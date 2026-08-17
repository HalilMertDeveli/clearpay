import 'package:clearpay/api/clearpay_client.dart';
import 'package:clearpay/auth/token_store.dart';
import 'package:clearpay/main.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  testWidgets('login is Turkish and shows the demo footer', (tester) async {
    final store = MemoryTokenStore();
    final api = ClearPayClient(
      store: store,
      baseUrl: 'http://localhost:5153',
      httpClient: MockClient((_) async => http.Response('{}', 500)),
    );
    await tester.pumpWidget(ClearPayApp(store: store, api: api));
    expect(find.text('Giriş'), findsOneWidget);
    expect(find.text('Demo — sahte banka gateway.'), findsOneWidget);
  });
}
