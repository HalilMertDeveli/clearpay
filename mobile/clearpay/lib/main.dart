import 'package:flutter/material.dart';

import 'api/clearpay_client.dart';
import 'auth/token_store.dart';
import 'firebase/bootstrap.dart';
import 'screens/login_screen.dart';
import 'screens/shell_screen.dart';
import 'theme.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initClearPayFirebase();
  final store = FileTokenStore();
  await store.load();
  runApp(ClearPayApp(
    store: store,
    api: ClearPayClient(store: store),
  ));
}

class ClearPayApp extends StatelessWidget {
  const ClearPayApp({
    super.key,
    required this.store,
    required this.api,
  });

  final TokenStore store;
  final ClearPayClient api;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ClearPay',
      debugShowCheckedModeBanner: false,
      locale: const Locale('tr'),
      theme: clearPayTheme,
      home: store.token == null
          ? LoginScreen(store: store, api: api)
          : ShellScreen(store: store, api: api),
    );
  }
}
