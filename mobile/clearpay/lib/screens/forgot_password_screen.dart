import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../auth/auth_session.dart';
import '../l10n/language_strip.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({
    super.key,
    required this.api,
    required this.auth,
    this.initialEmail,
  });

  final ClearPayClient api;
  final AuthSession auth;
  final String? initialEmail;

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  late final TextEditingController _lookup;
  final _token = TextEditingController();
  final _password = TextEditingController();
  String? _error;
  String? _info;
  bool _busy = false;
  bool _identityStep = false;

  @override
  void initState() {
    super.initState();
    _lookup = TextEditingController(text: widget.initialEmail ?? '');
  }

  @override
  void dispose() {
    _lookup.dispose();
    _token.dispose();
    _password.dispose();
    super.dispose();
  }

  bool get _looksLikeEmail => _lookup.text.contains('@');

  Future<void> _request() async {
    setState(() {
      _busy = true;
      _error = null;
      _info = null;
    });
    try {
      final raw = _lookup.text.trim();
      if (raw.isEmpty) {
        setState(() => _error = l10n(context).lookupEmpty);
        return;
      }
      if (_looksLikeEmail && widget.auth.isConfigured) {
        await widget.auth.sendPasswordResetEmail(raw);
        setState(() {
          _info = l10n(context).firebaseEmailSent;
          _identityStep = false;
        });
        return;
      }
      if (_looksLikeEmail) {
        await widget.api.forgotPassword(email: raw);
      } else {
        await widget.api.forgotPassword(phone: raw);
      }
      setState(() {
        _identityStep = true;
        _info = l10n(context).identityResetInfo;
      });
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

  Future<void> _reset() async {
    setState(() {
      _busy = true;
      _error = null;
      _info = null;
    });
    try {
      var email = _lookup.text.trim();
      if (!email.contains('@')) {
        setState(() => _error = l10n(context).resetNeedsEmail);
        return;
      }
      await widget.api.resetPassword(
        email: email,
        token: _token.text.trim(),
        newPassword: _password.text,
      );
      if (!mounted) {
        return;
      }
      Navigator.of(context).pop(true);
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
        title: Text(l.forgot),
        actions: const [Padding(padding: EdgeInsets.only(right: 8), child: LanguageStrip(light: true))],
      ),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Text(
            l.forgotLede,
            style: const TextStyle(color: navy),
          ),
          TextField(
            controller: _lookup,
            keyboardType: TextInputType.emailAddress,
            decoration: InputDecoration(
              labelText: l.lookupLabel,
              hintText: 'admin@clearpay.test veya 5550000001',
            ),
          ),
          const SizedBox(height: 16),
          FilledButton(
            onPressed: _busy ? null : _request,
            child: Text(_busy ? '…' : l.requestReset),
          ),
          if (_identityStep) ...[
            const SizedBox(height: 24),
            TextField(
              controller: _token,
              decoration: InputDecoration(labelText: l.resetCode),
            ),
            TextField(
              controller: _password,
              obscureText: true,
              decoration: InputDecoration(labelText: l.newPassword),
            ),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: _busy ? null : _reset,
              child: Text(l.updatePassword),
            ),
          ],
          if (_info != null) ...[
            const SizedBox(height: 12),
            Text(_info!, style: const TextStyle(color: teal)),
          ],
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: const TextStyle(color: Colors.red)),
          ],
          const DemoFooter(),
        ],
      ),
    );
  }
}
