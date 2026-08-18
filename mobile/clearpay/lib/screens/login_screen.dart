import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../auth/account_kind_store.dart';
import '../auth/auth_session.dart';
import '../auth/token_store.dart';
import '../debug_agent_log.dart';
import '../demo/tc_login.dart';
import '../firebase/bootstrap.dart';
import '../l10n/app_strings.dart';
import '../l10n/language_strip.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';
import 'forgot_password_screen.dart';
import 'register_screen.dart';
import 'shell_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({
    super.key,
    required this.store,
    required this.api,
    required this.kindStore,
    this.auth = const DisabledAuthSession(),
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;
  final AuthSession auth;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _email = TextEditingController();
  final _tc = TextEditingController();
  final _password = TextEditingController();
  int _tab = 0;
  String? _error;
  bool _busy = false;

  @override
  void dispose() {
    _email.dispose();
    _tc.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      var email = _email.text.trim();
      if (_tab == 1) {
        final mapped = resolveDemoTcEmail(_tc.text);
        if (mapped == null) {
          setState(() {
            _busy = false;
            _error = l10n(context).unknownTc;
          });
          return;
        }
        email = mapped;
      }
      try {
        await widget.api.login(
          email,
          _password.text,
          accountKind: widget.kindStore.kind,
        );
      } on ApiException catch (jwtError) {
        if (jwtError.status != 401 || !widget.auth.isConfigured) {
          rethrow;
        }
        try {
          final idToken = await widget.auth.signIn(email: email, password: _password.text);
          await widget.api.loginWithFirebase(
            idToken,
            accountKind: widget.kindStore.kind,
          );
        } on AuthException {
          throw jwtError;
        }
      }
      // #region agent log
      agentDebugLog(
        hypothesisId: 'B',
        location: 'login_screen.dart:_submit',
        message: 'login ok',
        data: {'hasJwtKind': widget.api.accountKind != null},
      );
      // #endregion
      if (!mounted) {
        return;
      }
      final jwtKind = widget.api.accountKind;
      if (jwtKind != null) {
        await widget.kindStore.save(jwtKind);
      }
      if (!mounted) {
        return;
      }
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ShellScreen(
            store: widget.store,
            api: widget.api,
            kindStore: widget.kindStore,
            auth: widget.auth,
          ),
        ),
      );
    } catch (e) {
      // #region agent log
      agentDebugLog(
        hypothesisId: e is ApiException ? 'F' : 'A',
        location: 'login_screen.dart:_submit',
        message: 'login failed',
        data: {
          'kind': e is ApiException ? 'api' : 'other',
          'status': e is ApiException ? e.status : null,
          'errorType': e.runtimeType.toString(),
        },
      );
      // #endregion
      if (e is ApiException) {
        setState(() => _error = e.message);
      } else {
        rethrow;
      }
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = l10n(context);
    return Scaffold(
      appBar: AppBar(
        title: const Text('ClearPay'),
        actions: const [Padding(padding: EdgeInsets.only(right: 8), child: LanguageStrip(light: true))],
      ),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Text(
            l.kindLogin(widget.kindStore.kind),
            style: const TextStyle(color: navy, fontWeight: FontWeight.w700, fontSize: 16),
          ),
          const SizedBox(height: 4),
          Text(
            l.signInLede,
            style: const TextStyle(color: navy),
          ),
          const SizedBox(height: 12),
          _FirestorePingBanner(l: l),
          const SizedBox(height: 16),
          SegmentedButton<int>(
            segments: [
              ButtonSegment(value: 0, label: Text(l.email)),
              ButtonSegment(value: 1, label: Text(l.tcDemo)),
            ],
            selected: {_tab},
            onSelectionChanged: (next) => setState(() => _tab = next.first),
          ),
          const SizedBox(height: 12),
          if (_tab == 0)
            TextField(
              controller: _email,
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.email],
              decoration: InputDecoration(labelText: l.email),
            )
          else ...[
            TextField(
              controller: _tc,
              keyboardType: TextInputType.number,
              decoration: InputDecoration(labelText: l.tcDemo),
            ),
            const SizedBox(height: 8),
            Text(
              l.tcHint,
              style: const TextStyle(color: muted, fontSize: 12),
            ),
          ],
          TextField(
            controller: _password,
            obscureText: true,
            decoration: InputDecoration(labelText: l.password),
          ),
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: const TextStyle(color: Colors.red)),
          ],
          const SizedBox(height: 16),
          FilledButton(
            onPressed: _busy ? null : _submit,
            child: Text(_busy ? '…' : l.signIn),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute<void>(
                  builder: (_) => ForgotPasswordScreen(
                    api: widget.api,
                    auth: widget.auth,
                    initialEmail: _tab == 0 ? _email.text : 'admin@clearpay.test',
                  ),
                ),
              );
            },
            child: Text(l.forgot),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute<void>(
                  builder: (_) => RegisterScreen(
                    store: widget.store,
                    api: widget.api,
                    kindStore: widget.kindStore,
                    auth: widget.auth,
                  ),
                ),
              );
            },
            child: Text(l.createAccount),
          ),
          const DemoFooter(),
        ],
      ),
    );
  }
}

class _FirestorePingBanner extends StatelessWidget {
  const _FirestorePingBanner({required this.l});

  final L l;

  @override
  Widget build(BuildContext context) {
    final kind = ClearPayFirestorePing.kind;
    final text = switch (kind) {
      ClearPayFirestorePingKind.wrote =>
        l.firestoreWrote(ClearPayFirestorePing.detail ?? kClearPayFirestorePingMessage),
      ClearPayFirestorePingKind.failed =>
        l.firestoreFailed(ClearPayFirestorePing.detail ?? 'error'),
      ClearPayFirestorePingKind.skipped => l.firestoreSkipped,
    };
    return DecoratedBox(
      decoration: BoxDecoration(
        color: kind == ClearPayFirestorePingKind.wrote
            ? const Color(0x1422A06B)
            : const Color(0x14C9A227),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Text(text, style: const TextStyle(color: navy, fontSize: 13)),
      ),
    );
  }
}
