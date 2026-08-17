import 'dart:ui' show PlatformDispatcher;

import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'api/clearpay_client.dart';
import 'auth/account_kind_store.dart';
import 'auth/auth_session.dart';
import 'auth/firebase_auth_session.dart';
import 'auth/token_store.dart';
import 'debug_agent_log.dart';
import 'firebase/bootstrap.dart';
import 'l10n/locale_scope.dart';
import 'l10n/locale_store.dart';
import 'platform/host.dart';
import 'screens/login_screen.dart';
import 'screens/mode_screen.dart';
import 'screens/shell_screen.dart';
import 'screens/splash_screen.dart';
import 'theme.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  FlutterError.onError = (details) {
    // #region agent log
    final text = details.exceptionAsString();
    agentDebugLog(
      hypothesisId: 'C',
      location: 'main.dart:FlutterError',
      message: 'flutter error',
      data: {
        'exception': text.length > 180 ? text.substring(0, 180) : text,
      },
    );
    // #endregion
    FlutterError.presentError(details);
  };
  PlatformDispatcher.instance.onError = (error, stack) {
    // #region agent log
    agentDebugLog(
      hypothesisId: 'B',
      location: 'main.dart:onError',
      message: 'zone error',
      data: {'errorType': error.runtimeType.toString()},
    );
    // #endregion
    return false;
  };
  // #region agent log
  agentDebugLog(
    hypothesisId: 'A',
    location: 'main.dart:main',
    message: 'pre-firebase',
    data: {'os': operatingSystemName},
  );
  // #endregion
  await initClearPayFirebase();
  final AuthSession auth = Firebase.apps.isNotEmpty
      ? FirebaseAuthSession()
      : const DisabledAuthSession();
  final store = SecureTokenStore();
  final kindStore = AccountKindStore();
  final localeStore = LocaleStore();
  await store.load();
  await kindStore.load();
  await localeStore.load();
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
    localeStore: localeStore,
    auth: auth,
  ));
}

class ClearPayApp extends StatefulWidget {
  const ClearPayApp({
    super.key,
    required this.store,
    required this.api,
    required this.kindStore,
    this.localeStore,
    this.auth = const DisabledAuthSession(),
    this.skipIntro = false,
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;
  final LocaleStore? localeStore;
  final AuthSession auth;
  final bool skipIntro;

  @override
  State<ClearPayApp> createState() => _ClearPayAppState();
}

class _ClearPayAppState extends State<ClearPayApp> {
  late final LocaleStore _locales = widget.localeStore ?? MemoryLocaleStore();

  @override
  Widget build(BuildContext context) {
    return LocaleScope(
      store: _locales,
      onChanged: () => setState(() {}),
      child: MaterialApp(
        title: 'ClearPay',
        debugShowCheckedModeBanner: false,
        locale: Locale(_locales.code),
        supportedLocales: const [
          Locale('tr'),
          Locale('en'),
          Locale('de'),
          Locale('fr'),
        ],
        localizationsDelegates: const [
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
        ],
        theme: clearPayTheme,
        home: widget.skipIntro
            ? (widget.store.token == null
                ? LoginScreen(
                    store: widget.store,
                    api: widget.api,
                    kindStore: widget.kindStore,
                    auth: widget.auth,
                  )
                : ShellScreen(
                    store: widget.store,
                    api: widget.api,
                    kindStore: widget.kindStore,
                    auth: widget.auth,
                  ))
            : _LaunchGate(
                store: widget.store,
                api: widget.api,
                kindStore: widget.kindStore,
                auth: widget.auth,
              ),
      ),
    );
  }
}

class _LaunchGate extends StatefulWidget {
  const _LaunchGate({
    required this.store,
    required this.api,
    required this.kindStore,
    required this.auth,
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;
  final AuthSession auth;

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
        auth: widget.auth,
      );
    }
    return LoginScreen(
      store: widget.store,
      api: widget.api,
      kindStore: widget.kindStore,
      auth: widget.auth,
    );
  }
}
