import 'dart:convert';
import 'dart:math';

import 'package:http/http.dart' as http;

import '../auth/token_store.dart';
import '../debug_agent_log.dart';
import '../platform/host.dart';

String defaultApiBase() {
  // CLEARPAY_API only. No MySQL dart-define: ledger is C# SQL Server (T-077 / T-061).
  const fromEnv = String.fromEnvironment('CLEARPAY_API');
  if (fromEnv.isNotEmpty) {
    return fromEnv;
  }
  if (isAndroidHost) {
    return 'http://10.0.2.2:5153';
  }
  return 'http://localhost:5153';
}

String newIdempotencyKey() {
  final rnd = Random.secure();
  final bytes = List<int>.generate(16, (_) => rnd.nextInt(256));
  bytes[6] = (bytes[6] & 0x0f) | 0x40;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;
  String hex(int i) => bytes[i].toRadixString(16).padLeft(2, '0');
  return '${hex(0)}${hex(1)}${hex(2)}${hex(3)}-'
      '${hex(4)}${hex(5)}-${hex(6)}${hex(7)}-'
      '${hex(8)}${hex(9)}-${hex(10)}${hex(11)}${hex(12)}${hex(13)}${hex(14)}${hex(15)}';
}

class ApiException implements Exception {
  ApiException(this.status, this.message);
  final int status;
  final String message;

  @override
  String toString() => message;
}

class WalletSnapshot {
  WalletSnapshot({
    required this.balance,
    required this.monthOutgoing,
    required this.monthIncoming,
    required this.isFrozen,
    required this.lastMovements,
  });

  final double balance;
  final double monthOutgoing;
  final double monthIncoming;
  final bool isFrozen;
  final List<WalletRow> lastMovements;

  factory WalletSnapshot.fromJson(Map<String, dynamic> json) {
    final raw = json['lastMovements'] as List<dynamic>? ?? [];
    return WalletSnapshot(
      balance: _num(json['balance']),
      monthOutgoing: _num(json['monthOutgoing']),
      monthIncoming: _num(json['monthIncoming']),
      isFrozen: json['isFrozen'] == true,
      lastMovements: raw
          .whereType<Map<String, dynamic>>()
          .map(WalletRow.fromJson)
          .toList(),
    );
  }
}

class WalletRow {
  WalletRow({
    required this.at,
    required this.kind,
    required this.amount,
    required this.correlationId,
  });

  final String at;
  final String kind;
  final double amount;
  final String correlationId;

  factory WalletRow.fromJson(Map<String, dynamic> json) => WalletRow(
        at: '${json['at'] ?? ''}',
        kind: '${json['kind'] ?? ''}',
        amount: _num(json['amount']),
        correlationId: '${json['correlationId'] ?? ''}',
      );
}

class MovementPage {
  MovementPage({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
  });

  final List<MovementRow> items;
  final int page;
  final int pageSize;
  final int totalCount;

  int get totalPages => pageSize <= 0 ? 1 : ((totalCount + pageSize - 1) / pageSize).ceil().clamp(1, 9999);

  factory MovementPage.fromJson(Map<String, dynamic> json) => MovementPage(
        items: _items(json, MovementRow.fromJson),
        page: json['page'] is num ? (json['page'] as num).toInt() : 1,
        pageSize: json['pageSize'] is num ? (json['pageSize'] as num).toInt() : 20,
        totalCount: json['totalCount'] is num ? (json['totalCount'] as num).toInt() : 0,
      );
}

class MovementRow {
  MovementRow({
    required this.at,
    required this.correlationId,
    required this.kind,
    required this.counterparty,
    required this.signedAmount,
    required this.status,
  });

  final String at;
  final String correlationId;
  final String kind;
  final String counterparty;
  final double signedAmount;
  final String status;

  factory MovementRow.fromJson(Map<String, dynamic> json) => MovementRow(
        at: '${json['at'] ?? ''}',
        correlationId: '${json['correlationId'] ?? ''}',
        kind: '${json['kind'] ?? ''}',
        counterparty: '${json['counterparty'] ?? ''}',
        signedAmount: _num(json['signedAmount']),
        status: '${json['status'] ?? ''}',
      );
}

class ReceiptSnapshot {
  ReceiptSnapshot({
    required this.correlationId,
    required this.at,
    required this.kind,
    required this.amount,
    required this.debitParty,
    required this.creditParty,
    this.description,
  });

  final String correlationId;
  final String at;
  final String kind;
  final double amount;
  final String debitParty;
  final String creditParty;
  final String? description;

  factory ReceiptSnapshot.fromJson(Map<String, dynamic> json) => ReceiptSnapshot(
        correlationId: '${json['correlationId'] ?? ''}',
        at: '${json['at'] ?? ''}',
        kind: '${json['kind'] ?? ''}',
        amount: _num(json['amount']),
        debitParty: '${json['debitParty'] ?? ''}',
        creditParty: '${json['creditParty'] ?? ''}',
        description: json['description'] as String?,
      );
}

double _num(dynamic value) {
  if (value is num) {
    return value.toDouble();
  }
  return double.tryParse('$value') ?? 0;
}

Map<String, dynamic> _map(dynamic value) => Map<String, dynamic>.from(value as Map);

List<T> _items<T>(Map<String, dynamic> json, T Function(Map<String, dynamic>) map) {
  final raw = json['items'] as List<dynamic>? ?? [];
  return raw.map((e) => map(_map(e))).toList();
}

class CardSnapshot {
  CardSnapshot({required this.last4, required this.label, required this.accountHint});

  final String last4;
  final String label;
  final String accountHint;

  factory CardSnapshot.fromJson(Map<String, dynamic> json) => CardSnapshot(
        last4: '${json['last4'] ?? ''}',
        label: '${json['label'] ?? ''}',
        accountHint: '${json['accountHint'] ?? json['last4'] ?? ''}',
      );
}

class OutboxRow {
  OutboxRow({required this.id, required this.type, required this.correlationId, required this.occurredAt});

  final String id;
  final String type;
  final String correlationId;
  final String occurredAt;

  factory OutboxRow.fromJson(Map<String, dynamic> json) => OutboxRow(
        id: '${json['id'] ?? ''}',
        type: '${json['type'] ?? ''}',
        correlationId: '${json['correlationId'] ?? ''}',
        occurredAt: '${json['occurredAt'] ?? ''}',
      );
}

class AuditRow {
  AuditRow({required this.actorUserId, required this.action, required this.correlationId, required this.createdAt});

  final String actorUserId;
  final String action;
  final String correlationId;
  final String createdAt;

  factory AuditRow.fromJson(Map<String, dynamic> json) => AuditRow(
        actorUserId: '${json['actorUserId'] ?? ''}',
        action: '${json['action'] ?? ''}',
        correlationId: '${json['correlationId'] ?? ''}',
        createdAt: '${json['createdAt'] ?? ''}',
      );
}

Map<String, dynamic>? jwtPayload(String? token) {
  if (token == null || token.isEmpty) {
    return null;
  }
  final parts = token.split('.');
  if (parts.length < 2) {
    return null;
  }
  var payload = parts[1].replaceAll('-', '+').replaceAll('_', '/');
  switch (payload.length % 4) {
    case 2:
      payload += '==';
    case 3:
      payload += '=';
  }
  try {
    return jsonDecode(utf8.decode(base64.decode(payload))) as Map<String, dynamic>;
  } catch (_) {
    return null;
  }
}

bool jwtIsAdmin(String? token) {
  final json = jwtPayload(token);
  if (json == null) {
    return false;
  }
  const roleKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
  final role = json[roleKey] ?? json['role'];
  if (role is List) {
    return role.contains('Admin');
  }
  return role == 'Admin';
}

String? jwtEmail(String? token) {
  final json = jwtPayload(token);
  if (json == null) {
    return null;
  }
  const emailKey = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
  final email = json['email'] ?? json[emailKey] ?? json['unique_name'];
  if (email is String && email.isNotEmpty) {
    return email;
  }
  return null;
}

String? jwtAccountKind(String? token) {
  final json = jwtPayload(token);
  if (json == null) {
    return null;
  }
  final kind = json['account_kind'];
  if (kind is String && kind.isNotEmpty) {
    return kind;
  }
  return null;
}

/// JWT client only. Does not store a second balance (no Hive).
class ClearPayClient {
  ClearPayClient({
    required this.store,
    http.Client? httpClient,
    String? baseUrl,
    this.onUnauthorized,
  })  : _http = httpClient ?? http.Client(),
        baseUrl = baseUrl ?? defaultApiBase();

  final TokenStore store;
  final http.Client _http;
  final String baseUrl;
  Future<void> Function()? onUnauthorized;

  bool get isAdmin => jwtIsAdmin(store.token);

  String? get email => jwtEmail(store.token);

  String? get accountKind => jwtAccountKind(store.token);

  Future<void> login(String email, String password, {String? accountKind}) async {
    final response = await _http.post(
      _uri('/api/token'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email.trim(),
        'password': password,
        if (accountKind != null && accountKind.isNotEmpty) 'accountKind': accountKind,
      }),
    );
    // #region agent log
    agentDebugLog(
      hypothesisId: 'F',
      location: 'clearpay_client.dart:login',
      message: 'token response',
      data: {
        'status': response.statusCode,
        'host': _uri('/api/token').host,
        'kindSent': accountKind != null && accountKind.isNotEmpty,
      },
    );
    // #endregion
    if (response.statusCode != 200) {
      throw ApiException(response.statusCode, 'E-posta veya şifre hatalı.');
    }
    await _takeToken(response.body);
  }

  Future<void> register({
    required String fullName,
    required String email,
    required String password,
    required String confirmPassword,
    String? accountKind,
  }) async {
    final response = await _http.post(
      _uri('/api/register'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'fullName': fullName.trim(),
        'email': email.trim(),
        'password': password,
        'confirmPassword': confirmPassword,
        if (accountKind != null && accountKind.isNotEmpty) 'accountKind': accountKind,
      }),
    );
    if (response.statusCode != 201) {
      throw ApiException(response.statusCode, _problem(response.body, response.statusCode));
    }
    await _takeToken(response.body);
  }

  Future<void> _takeToken(String body) async {
    final json = jsonDecode(body) as Map<String, dynamic>;
    final token = json['access_token'] as String?;
    if (token == null || token.isEmpty) {
      throw ApiException(500, 'Token alınamadı.');
    }
    await store.save(token);
  }

  Future<WalletSnapshot> wallet() async {
    final json = await _get('/api/wallet');
    return WalletSnapshot.fromJson(json);
  }

  Future<MovementPage> movements({
    String? kind,
    String? from,
    String? to,
    int page = 1,
    int pageSize = 20,
  }) async {
    final query = <String, String>{
      if (kind != null && kind.isNotEmpty && kind != 'all') 'kind': kind,
      if (from != null && from.isNotEmpty) 'from': from,
      if (to != null && to.isNotEmpty) 'to': to,
      'page': '$page',
      'pageSize': '$pageSize',
    };
    final json = await _get('/api/movements', query);
    return MovementPage.fromJson(json);
  }

  Future<ReceiptSnapshot> receipt(String correlationId) async {
    final json = await _get('/api/receipts/$correlationId');
    return ReceiptSnapshot.fromJson(json);
  }

  Future<List<CardSnapshot>> cards() async {
    final json = await _get('/api/cards');
    return _items(json, CardSnapshot.fromJson);
  }

  Future<void> addCard({required String last4, required String label}) async {
    await _postJson('/api/cards', {'last4': last4, 'label': label}, money: false);
  }

  Future<void> transfer({
    required String recipient,
    required double amount,
    String? description,
  }) async {
    await _postMoney(
      '/api/transfers',
      {
        'recipient': recipient.trim(),
        'amount': amount,
        'description': description ?? 'demo',
      },
    );
  }

  Future<void> topUp({required double amount, required String account}) async {
    await _postMoney('/api/topup', {'amount': amount, 'account': account});
  }

  Future<void> withdraw({required double amount, required String account}) async {
    await _postMoney('/api/withdraw', {'amount': amount, 'account': account});
  }

  Future<void> freeze(String email) async {
    await _postJson('/api/admin/freeze', {'email': email.trim()}, money: false);
  }

  Future<void> unfreeze(String email) async {
    await _postJson('/api/admin/unfreeze', {'email': email.trim()}, money: false);
  }

  Future<List<OutboxRow>> failedOutbox() async {
    final json = await _get('/api/admin/outbox');
    return _items(json, OutboxRow.fromJson);
  }

  Future<void> requeue(String id) async {
    await _postJson('/api/admin/outbox/$id/requeue', {}, money: false);
  }

  Future<List<AuditRow>> audit({String? actor, String? correlationId}) async {
    final json = await _get('/api/admin/audit', {
      if (actor != null && actor.isNotEmpty) 'actor': actor,
      if (correlationId != null && correlationId.isNotEmpty) 'correlationId': correlationId,
    });
    return _items(json, AuditRow.fromJson);
  }

  Uri _uri(String path) => Uri.parse('$baseUrl$path');

  Map<String, String> _headers({required bool money}) {
    final token = store.token;
    if (token == null || token.isEmpty) {
      throw ApiException(401, 'Oturum yok. Yeniden giriş yapın.');
    }
    return {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
      if (money) 'Idempotency-Key': newIdempotencyKey(),
    };
  }

  Future<Map<String, dynamic>> _get(String path, [Map<String, String>? query]) async {
    var uri = _uri(path);
    if (query != null && query.isNotEmpty) {
      uri = uri.replace(queryParameters: query);
    }
    final response = await _http.get(uri, headers: _headers(money: false));
    return await _decode(response);
  }

  Future<Map<String, dynamic>> _postMoney(String path, Map<String, dynamic> body) =>
      _postJson(path, body, money: true);

  Future<Map<String, dynamic>> _postJson(
    String path,
    Map<String, dynamic> body, {
    required bool money,
  }) async {
    final response = await _http.post(
      _uri(path),
      headers: _headers(money: money),
      body: jsonEncode(body),
    );
    return await _decode(response);
  }

  Future<Map<String, dynamic>> _decode(http.Response response) async {
    if (response.statusCode == 401) {
      final handler = onUnauthorized;
      if (handler != null) {
        await handler();
      }
      throw ApiException(401, 'Oturum süresi doldu.');
    }
    if (response.statusCode == 409) {
      throw ApiException(
        409,
        'Bu işlem zaten işlendi. Cüzdan ikinci kez kesilmedi.',
      );
    }
    if (response.statusCode == 201 ||
        response.statusCode == 200 ||
        response.statusCode == 202) {
      if (response.body.isEmpty) {
        return {};
      }
      final decoded = jsonDecode(response.body);
      return decoded is Map<String, dynamic> ? decoded : {};
    }
    throw ApiException(response.statusCode, _problem(response.body, response.statusCode));
  }

  String _problem(String body, int status) {
    try {
      final json = jsonDecode(body) as Map<String, dynamic>;
      final errors = json['errors'];
      if (errors is Map && errors.isNotEmpty) {
        return errors.values.expand((e) => e is List ? e : [e]).join(' ');
      }
      return '${json['detail'] ?? json['title'] ?? 'İstek başarısız ($status)'}';
    } catch (_) {
      return 'İstek başarısız ($status).';
    }
  }
}

