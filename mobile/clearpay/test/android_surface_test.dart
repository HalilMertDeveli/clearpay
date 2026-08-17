import 'dart:convert';

import 'package:clearpay/api/clearpay_client.dart';
import 'package:clearpay/api/wallet_live_hub.dart';
import 'package:clearpay/auth/account_kind_store.dart';
import 'package:clearpay/auth/token_store.dart';
import 'package:clearpay/demo/tc_login.dart';
import 'package:clearpay/main.dart';
import 'package:clearpay/platform/host.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

String _jwt({required String email}) {
  final header = base64Url.encode(utf8.encode('{"alg":"none"}'));
  final payload = base64Url.encode(
    utf8.encode(jsonEncode({'email': email, 'account_kind': 'Bireysel'})),
  );
  return '$header.$payload.sig';
}

void main() {
  test('Android emulator API base is the host loopback alias', () {
    expect(apiBaseFor(android: true), 'http://10.0.2.2:5153');
    expect(apiBaseFor(android: false), 'http://localhost:5153');
    expect(
      apiBaseFor(android: true, fromEnv: 'http://example.test:5153'),
      'http://example.test:5153',
    );
    expect(isAndroidHost, isFalse);
    expect(defaultApiBase(), 'http://localhost:5153');
  });

  test('VM host skips SignalR and JWT-polls instead', () async {
    final hub = WalletLiveHub();
    var ticks = 0;
    await hub.connect(
      baseUrl: 'http://localhost:5153',
      token: () => 'jwt',
      onChanged: () => ticks++,
      pollEvery: const Duration(milliseconds: 40),
    );
    await Future<void>.delayed(const Duration(milliseconds: 130));
    expect(ticks, greaterThanOrEqualTo(1));
    await hub.dispose();
    final afterDispose = ticks;
    await Future<void>.delayed(const Duration(milliseconds: 80));
    expect(ticks, afterDispose);
  });

  test('MemoryTokenStore holds JWT only — not a local balance', () async {
    final store = MemoryTokenStore();
    await store.save(_jwt(email: 'admin@clearpay.test'));
    expect(store.token, isNotNull);
    expect(store.token!.split('.'), hasLength(3));
    expect(store.token, isNot(contains('"balance"')));
    await store.clear();
    expect(store.token, isNull);
  });

  test('duplicate transfer surfaces 409 without a second cash box', () async {
    final store = MemoryTokenStore(_jwt(email: 'a@clearpay.test'));
    final api = ClearPayClient(
      store: store,
      baseUrl: 'http://10.0.2.2:5153',
      httpClient: MockClient(
        (_) async => http.Response('{"title":"Conflict"}', 409),
      ),
    );
    await expectLater(
      api.transfer(recipient: 'b@clearpay.test', amount: 1),
      throwsA(isA<ApiException>().having((e) => e.status, 'status', 409)),
    );
  });

  testWidgets('demo TC tab posts mapped email to JWT then shows overview', (tester) async {
    final store = MemoryTokenStore();
    var postedEmail = '';
    final api = ClearPayClient(
      store: store,
      baseUrl: 'http://10.0.2.2:5153',
      httpClient: MockClient((request) async {
        if (request.url.path == '/api/token') {
          postedEmail = jsonDecode(request.body)['email'] as String;
          return http.Response(
            jsonEncode({'access_token': _jwt(email: postedEmail)}),
            200,
          );
        }
        if (request.url.path == '/api/wallet') {
          return http.Response(
            jsonEncode({
              'balance': 0,
              'monthOutgoing': 0,
              'monthIncoming': 0,
              'isFrozen': false,
              'lastMovements': const <Object>[],
            }),
            200,
          );
        }
        return http.Response('{}', 404);
      }),
    );

    await tester.pumpWidget(
      ClearPayApp(
        store: store,
        api: api,
        kindStore: MemoryAccountKindStore(),
        skipIntro: true,
      ),
    );

    await tester.tap(find.text('TC (demo)').first);
    await tester.pump();
    expect(resolveDemoTcEmail('10000000146'), 'admin@clearpay.test');

    await tester.enterText(find.byType(TextField).at(0), '10000000146');
    await tester.enterText(find.byType(TextField).at(1), 'Deneme123');
    await tester.tap(find.widgetWithText(FilledButton, 'Giriş'));
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 100));

    expect(postedEmail, 'admin@clearpay.test');
    expect(find.text('Hızlı işlemler'), findsOneWidget);
    expect(find.text('0,00 ₺'), findsOneWidget);
    expect(find.textContaining('Kartlarım'), findsNothing);
    await tester.pumpWidget(const SizedBox.shrink());
    await tester.pump();
  });
}
