import 'dart:convert';

import 'package:clearpay/api/clearpay_client.dart';
import 'package:clearpay/auth/account_kind_store.dart';
import 'package:clearpay/auth/token_store.dart';
import 'package:clearpay/demo/tc_login.dart';
import 'package:clearpay/main.dart';
import 'package:clearpay/qr/pay_uri.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

String _jwt({required String email, bool admin = false, String kind = 'Bireysel'}) {
  final header = base64Url.encode(utf8.encode('{"alg":"none"}'));
  final payload = base64Url.encode(
    utf8.encode(
      jsonEncode({
        'email': email,
        'account_kind': kind,
        if (admin) 'role': 'Admin',
      }),
    ),
  );
  return '$header.$payload.sig';
}

ClearPayClient _client(TokenStore store, {http.Client? httpClient}) {
  return ClearPayClient(
    store: store,
    baseUrl: 'http://localhost:5153',
    httpClient: httpClient ?? MockClient((_) async => http.Response('{}', 500)),
  );
}

ClearPayApp _app({
  required TokenStore store,
  ClearPayClient? api,
  AccountKindStore? kindStore,
}) {
  return ClearPayApp(
    store: store,
    api: api ?? _client(store),
    kindStore: kindStore ?? MemoryAccountKindStore(),
    skipIntro: true,
  );
}

void main() {
  test('demo TC maps only the documented seed to admin email', () {
    expect(resolveDemoTcEmail('10000000146'), 'admin@clearpay.test');
    expect(resolveDemoTcEmail('100-000-00146'), 'admin@clearpay.test');
    expect(resolveDemoTcEmail('12345678901'), isNull);
  });

  test('pay URI encodes and parses ClearPay payload', () {
    final encoded = const PayUri(to: 'admin@clearpay.test', amount: '10.5').encode();
    expect(encoded, contains('clearpay://pay'));
    expect(PayUri.tryParse(encoded)?.to, 'admin@clearpay.test');
    expect(PayUri.tryParse(encoded)?.amount, '10.5');
    expect(PayUri.tryParse('admin@clearpay.test')?.to, 'admin@clearpay.test');
  });

  testWidgets('login is Turkish and shows the demo footer', (tester) async {
    final store = MemoryTokenStore();
    await tester.pumpWidget(_app(store: store));
    expect(find.text('Giriş'), findsOneWidget);
    expect(find.text('E-posta'), findsWidgets);
    expect(find.text('TC (demo)'), findsOneWidget);
    expect(find.text('Demo — sahte banka gateway.'), findsOneWidget);
  });

  testWidgets('splash then Bireysel and Kurumsal cards', (tester) async {
    final store = MemoryTokenStore();
    await tester.pumpWidget(
      ClearPayApp(
        store: store,
        api: _client(store),
        kindStore: MemoryAccountKindStore(),
      ),
    );
    expect(find.text('ClearPay'), findsOneWidget);
    await tester.pump(const Duration(milliseconds: 1500));
    await tester.pumpAndSettle();
    expect(find.text('Bireysel'), findsOneWidget);
    expect(find.text('Kurumsal'), findsOneWidget);
    await tester.tap(find.text('Bireysel'));
    await tester.pumpAndSettle();
    expect(find.text('Giriş'), findsOneWidget);
    expect(find.text('TC (demo)'), findsOneWidget);
  });

  testWidgets('logged-in shell has left drawer for existing ops, not Yapı Kredi', (tester) async {
    final store = MemoryTokenStore(_jwt(email: 'admin@clearpay.test', admin: true));
    final api = _client(
      store,
      httpClient: MockClient((request) async {
        if (request.url.path == '/api/wallet') {
          return http.Response(
            jsonEncode({
              'balance': 80.5,
              'monthOutgoing': 10,
              'monthIncoming': 20,
              'isFrozen': false,
              'lastMovements': [
                {
                  'at': '2026-08-17',
                  'kind': 'Havale',
                  'amount': -5,
                  'correlationId': 'corr-1',
                },
              ],
            }),
            200,
          );
        }
        return http.Response('{}', 404);
      }),
    );
    await tester.pumpWidget(_app(store: store, api: api));
    await tester.pump();
    await tester.pump();

    expect(find.text('80,50 ₺'), findsOneWidget);
    expect(find.text('Hızlı işlemler'), findsOneWidget);
    expect(find.text('Yükle'), findsWidgets);
    expect(find.text('Çek'), findsOneWidget);
    expect(find.text('QR al'), findsOneWidget);
    expect(find.text('QR öde'), findsOneWidget);
    expect(find.text('Daha fazla'), findsOneWidget);
    expect(find.text('FAST'), findsOneWidget);
    expect(find.text('Bireysel'), findsWidgets);
    expect(find.textContaining('Yapı Kredi'), findsNothing);
    expect(find.textContaining('YapıKredi'), findsNothing);
    expect(find.text('World'), findsNothing);
    expect(find.text('Jet QR'), findsNothing);
    expect(find.text('World Pay'), findsNothing);

    await tester.tap(find.byIcon(Icons.menu));
    await tester.pumpAndSettle();

    expect(find.byType(NavigationDrawer), findsOneWidget);
    expect(find.text('ClearPay'), findsWidgets);
    expect(find.text('admin@clearpay.test'), findsOneWidget);
    expect(find.text('Özet'), findsWidgets);
    expect(find.text('Havale'), findsWidgets);
    expect(find.text('Yükle / Çek'), findsOneWidget);
    expect(find.text('Hareketler'), findsWidgets);
    expect(find.text('Dekont'), findsOneWidget);
    expect(find.text('Admin'), findsWidgets);
    expect(find.text('Çıkış'), findsOneWidget);
    expect(find.text('Demo — sahte banka gateway.'), findsWidgets);
  });

  test('401 invokes onUnauthorized then throws', () async {
    var called = false;
    final store = MemoryTokenStore(_jwt(email: 'a@clearpay.test'));
    final api = ClearPayClient(
      store: store,
      baseUrl: 'http://localhost:5153',
      httpClient: MockClient((_) async => http.Response('{"title":"Unauthorized"}', 401)),
      onUnauthorized: () async {
        called = true;
      },
    );
    await expectLater(api.wallet(), throwsA(isA<ApiException>()));
    expect(called, isTrue);
  });

  test('receipt JSON keeps last4 hint', () {
    final receipt = ReceiptSnapshot.fromJson({
      'correlationId': 'aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001',
      'at': '2026-08-17',
      'kind': 'TopUp',
      'amount': 25,
      'debitParty': 'treasury',
      'creditParty': 'admin@clearpay.test',
      'description': '****4242',
      'instrumentHint': '****4242',
    });
    expect(receipt.correlationId, 'aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001');
    expect(receipt.instrumentHint, '****4242');
  });

  test('gateway timeout does not look like a posted receipt', () async {
    final store = MemoryTokenStore(_jwt(email: 'a@clearpay.test'));
    final api = ClearPayClient(
      store: store,
      baseUrl: 'http://localhost:5153',
      httpClient: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'correlationId': 'aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001',
            'detail': 'Gateway timed out',
          }),
          202,
        ),
      ),
    );
    await expectLater(
      api.topUp(amount: 10, account: 'TIMEOUT'),
      throwsA(
        isA<ApiException>().having((e) => e.status, 'status', 202),
      ),
    );
  });
}
