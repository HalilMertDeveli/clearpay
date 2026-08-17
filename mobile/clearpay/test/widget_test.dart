import 'dart:convert';

import 'package:clearpay/api/clearpay_client.dart';
import 'package:clearpay/auth/token_store.dart';
import 'package:clearpay/main.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

String _jwt({required String email, bool admin = false}) {
  final header = base64Url.encode(utf8.encode('{"alg":"none"}'));
  final payload = base64Url.encode(
    utf8.encode(
      jsonEncode({
        'email': email,
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

void main() {
  testWidgets('login is Turkish and shows the demo footer', (tester) async {
    final store = MemoryTokenStore();
    await tester.pumpWidget(ClearPayApp(store: store, api: _client(store)));
    expect(find.text('Giriş'), findsOneWidget);
    expect(find.text('Demo — sahte banka gateway.'), findsOneWidget);
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
    await tester.pumpWidget(ClearPayApp(store: store, api: api));
    await tester.pump();
    await tester.pump();

    expect(find.text('80,50 ₺'), findsOneWidget);
    expect(find.text('Hızlı işlemler'), findsOneWidget);
    expect(find.text('Yükle'), findsWidgets);
    expect(find.text('Çek'), findsOneWidget);
    expect(find.textContaining('Yapı Kredi'), findsNothing);
    expect(find.textContaining('YapıKredi'), findsNothing);
    expect(find.text('World'), findsNothing);

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
}
