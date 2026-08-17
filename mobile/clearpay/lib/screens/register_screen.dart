import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../auth/account_kind_store.dart';
import '../auth/auth_session.dart';
import '../auth/token_store.dart';
import '../l10n/language_strip.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';
import 'forgot_password_screen.dart';
import 'shell_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({
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
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _password = TextEditingController();
  final _confirm = TextEditingController();
  String? _error;
  bool _busy = false;

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _phone.dispose();
    _password.dispose();
    _confirm.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      if (_password.text != _confirm.text) {
        setState(() => _error = l10n(context).passwordsMismatch);
        return;
      }
      if (_phone.text.trim().isEmpty) {
        setState(() => _error = l10n(context).phoneRequired);
        return;
      }
      if (!widget.auth.isConfigured) {
        setState(() => _error = DisabledAuthSession.notConfigured);
        return;
      }
      String idToken;
      try {
        idToken = await widget.auth.register(
          email: _email.text,
          password: _password.text,
          fullName: _name.text,
        );
      } on AuthException catch (e) {
        if (e.code != 'email-already-in-use') {
          rethrow;
        }
        idToken = await widget.auth.signIn(
          email: _email.text,
          password: _password.text,
        );
      }
      await widget.api.loginWithFirebase(
        idToken,
        fullName: _name.text,
        phone: _phone.text,
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
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute<void>(
          builder: (_) => ShellScreen(
            store: widget.store,
            api: widget.api,
            kindStore: widget.kindStore,
            auth: widget.auth,
          ),
        ),
        (_) => false,
      );
    } on AuthException catch (e) {
      setState(() => _error = e.message);
    } on ApiException catch (e) {
      setState(() => _error = e.message);
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
        title: Text(l.registerTitle),
        actions: const [Padding(padding: EdgeInsets.only(right: 8), child: LanguageStrip(light: true))],
      ),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Text(
            l.kindRegister(widget.kindStore.kind),
            style: const TextStyle(color: navy, fontWeight: FontWeight.w600),
          ),
          TextField(controller: _name, decoration: InputDecoration(labelText: l.fullName)),
          TextField(
            controller: _email,
            keyboardType: TextInputType.emailAddress,
            decoration: InputDecoration(labelText: l.email),
          ),
          TextField(
            controller: _phone,
            keyboardType: TextInputType.phone,
            decoration: InputDecoration(
              labelText: l.phone,
              hintText: '5550000001',
            ),
          ),
          TextField(
            controller: _password,
            obscureText: true,
            decoration: InputDecoration(labelText: l.password),
          ),
          TextField(
            controller: _confirm,
            obscureText: true,
            decoration: InputDecoration(labelText: l.confirmPassword),
          ),
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: const TextStyle(color: Colors.red)),
          ],
          const SizedBox(height: 16),
          FilledButton(
            onPressed: _busy ? null : _submit,
            child: Text(_busy ? '…' : l.createAccount),
          ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(l.haveAccount),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute<void>(
                  builder: (_) => ForgotPasswordScreen(
                    api: widget.api,
                    auth: widget.auth,
                    initialEmail: _email.text,
                  ),
                ),
              );
            },
            child: Text(l.forgot),
          ),
          const DemoFooter(),
        ],
      ),
    );
  }
}
