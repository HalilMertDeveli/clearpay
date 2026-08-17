import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../auth/account_kind_store.dart';
import '../auth/token_store.dart';
import '../theme.dart';
import 'shell_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({
    super.key,
    required this.store,
    required this.api,
    required this.kindStore,
  });

  final TokenStore store;
  final ClearPayClient api;
  final AccountKindStore kindStore;

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _confirm = TextEditingController();
  String? _error;
  bool _busy = false;

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
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
      await widget.api.register(
        fullName: _name.text,
        email: _email.text,
        password: _password.text,
        confirmPassword: _confirm.text,
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
          ),
        ),
        (_) => false,
      );
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
    return Scaffold(
      appBar: AppBar(title: const Text('Hesap oluştur')),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Text(
            '${widget.kindStore.kind} kayıt — aynı 8 ekran',
            style: const TextStyle(color: navy, fontWeight: FontWeight.w600),
          ),
          TextField(controller: _name, decoration: const InputDecoration(labelText: 'Ad')),
          TextField(
            controller: _email,
            keyboardType: TextInputType.emailAddress,
            decoration: const InputDecoration(labelText: 'E-posta'),
          ),
          TextField(
            controller: _password,
            obscureText: true,
            decoration: const InputDecoration(labelText: 'Şifre'),
          ),
          TextField(
            controller: _confirm,
            obscureText: true,
            decoration: const InputDecoration(labelText: 'Şifre tekrar'),
          ),
          if (_error != null) ...[
            const SizedBox(height: 12),
            Text(_error!, style: const TextStyle(color: Colors.red)),
          ],
          const SizedBox(height: 16),
          FilledButton(
            onPressed: _busy ? null : _submit,
            child: Text(_busy ? '…' : 'Hesap oluştur'),
          ),
          const DemoFooter(),
        ],
      ),
    );
  }
}
