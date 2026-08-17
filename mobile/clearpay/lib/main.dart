import 'package:flutter/material.dart';

import 'api/clearpay_client.dart';
import 'auth/account_kind_store.dart';
import 'auth/token_store.dart';
import 'debug_agent_log.dart';
import 'firebase/bootstrap.dart';
import 'platform/host.dart';
import 'screens/login_screen.dart';
import 'screens/mode_screen.dart';
import 'screens/shell_screen.dart';
import 'screens/splash_screen.dart';
import 'theme.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initClearPayFirebase();
  final store = SecureTokenStore();
  final kindStore = AccountKindStore();
  await store.load();
  await kindStore.load();
  // #region agent log
  agentDebugLog(
    hypothesisId: 'A',
    location: 'main.dart:main',
    message: 'launch',
    data: {
      'os': operatingSystemName,
      'apiBase': defaultApiBase(),
      'hasToken': store.token != null,
      'kind': kindStore.kind,
    },
  );
  // #endregion
  runApp(ClearPayApp(
    store: store,
    api: ClearPayClient(store: store),
    kindStore: kindStore,
  ));
}

class ClearPayApp extends StatelessWidget {
  const ClearPayApp({
    super.key,
    required this.store,
    required this.api,
    required this.kindStore,
    this.skipIntro = false,
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;
  final bool skipIntro;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ClearPay',
      debugShowCheckedModeBanner: false,
      locale: const Locale('tr'),
      theme: clearPayTheme,
      home: skipIntro
          ? (store.token == null
              ? LoginScreen(store: store, api: api, kindStore: kindStore)
              : ShellScreen(store: store, api: api, kindStore: kindStore))
          : _LaunchGate(store: store, api: api, kindStore: kindStore),
    );
  }
}

class _LaunchGate extends StatefulWidget {
  const _LaunchGate({
    required this.store,
    required this.api,
    required this.kindStore,
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;

  @override
  State<_LaunchGate> createState() => _LaunchGateState();
}

class _LaunchGateState extends State<_LaunchGate> {
  var _phase = 0;

  @override
  Widget build(BuildContext context) {
    if (_phase == 0) {
      return SplashScreen(
        onFinished: () {
          if (mounted) {
            setState(() => _phase = widget.store.token == null ? 1 : 2);
          }
        },
      );
    }
    if (_phase == 1) {
      return ModeScreen(
        store: widget.kindStore,
        onPicked: (kind) async {
          await widget.kindStore.save(kind);
          if (mounted) {
            setState(() => _phase = 3);
          }
        },
      );
    }
    if (_phase == 2) {
      return ShellScreen(
        store: widget.store,
        api: widget.api,
        kindStore: widget.kindStore,
      );
    }
    return LoginScreen(
      store: widget.store,
      api: widget.api,
      kindStore: widget.kindStore,
    );
  }
}
