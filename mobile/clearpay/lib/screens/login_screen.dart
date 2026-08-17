import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../auth/account_kind_store.dart';
import '../auth/token_store.dart';
import '../debug_agent_log.dart';
import '../demo/tc_login.dart';
import '../theme.dart';
import 'register_screen.dart';
import 'shell_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({
    super.key,
    required this.store,
    required this.api,
    required this.kindStore,
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;

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
      var email = _email.text;
      if (_tab == 1) {
        final mapped = resolveDemoTcEmail(_tc.text);
        if (mapped == null) {
          setState(() {
            _busy = false;
            _error = 'Bu demo TC tanımlı değil. Mernis yok. Seed: $demoAdminTc veya e-posta ile girin.';
          });
          return;
        }
        email = mapped;
      }
      await widget.api.login(
        email,
        _password.text,
        accountKind: widget.kindStore.kind,
      );
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
    return Scaffold(
      appBar: AppBar(title: const Text('ClearPay')),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Text(
            '${widget.kindStore.kind} giriş',
            style: const TextStyle(color: navy, fontWeight: FontWeight.w700, fontSize: 16),
          ),
          const SizedBox(height: 4),
          const Text(
            'Aynı SQL defteri. Bakiye telefonda tutulmaz.',
            style: TextStyle(color: navy),
          ),
          const SizedBox(height: 16),
          SegmentedButton<int>(
            segments: const [
              ButtonSegment(value: 0, label: Text('E-posta')),
              ButtonSegment(value: 1, label: Text('TC (demo)')),
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
              decoration: const InputDecoration(labelText: 'E-posta'),
            )
          else ...[
            TextField(
              controller: _tc,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(labelText: 'TC (demo)'),
            ),
            const SizedBox(height: 8),
            const Text(
              'Mernis değil. Demo seed 10000000146 → admin@clearpay.test',
              style: TextStyle(color: muted, fontSize: 12),
            ),
          ],
          TextField(
            controller: _password,
            obscureText: true,
            decoration: const InputDecoration(labelText: 'Şifre'),
          ),
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: const TextStyle(color: Colors.red)),
          ],
          const SizedBox(height: 16),
          FilledButton(
            onPressed: _busy ? null : _submit,
            child: Text(_busy ? '…' : 'Giriş'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute<void>(
                  builder: (_) => RegisterScreen(
                    store: widget.store,
                    api: widget.api,
                    kindStore: widget.kindStore,
                  ),
                ),
              );
            },
            child: const Text('Hesap oluştur'),
          ),
          const DemoFooter(),
        ],
      ),
    );
  }
}
