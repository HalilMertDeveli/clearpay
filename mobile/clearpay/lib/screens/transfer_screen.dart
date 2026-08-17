import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../theme.dart';

class TransferScreen extends StatefulWidget {
  const TransferScreen({super.key, required this.api});

  final ClearPayClient api;

  @override
  State<TransferScreen> createState() => _TransferScreenState();
}

class _TransferScreenState extends State<TransferScreen> {
  final _recipient = TextEditingController();
  final _amount = TextEditingController();
  final _description = TextEditingController(text: 'demo');
  double? _balance;
  String? _message;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _loadBalance();
  }

  @override
  void dispose() {
    _recipient.dispose();
    _amount.dispose();
    _description.dispose();
    super.dispose();
  }

  Future<void> _loadBalance() async {
    try {
      final wallet = await widget.api.wallet();
      if (!mounted) {
        return;
      }
      setState(() => _balance = wallet.balance);
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _message = e.message);
    }
  }

  Future<void> _send() async {
    final amount = double.tryParse(_amount.text.replaceAll(',', '.'));
    if (amount == null || amount <= 0) {
      setState(() => _message = 'Geçerli tutar girin.');
      return;
    }
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Havale onayı'),
        content: Text('${_recipient.text} hesabına ${formatTry(amount)} gönderilsin mi?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('İptal')),
          FilledButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Gönder')),
        ],
      ),
    );
    if (ok != true) {
      return;
    }
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      await widget.api.transfer(
        recipient: _recipient.text,
        amount: amount,
        description: _description.text,
      );
      await _loadBalance();
      setState(() => _message = 'Havale alındı. Bakiye yenilendi.');
    } on ApiException catch (e) {
      setState(() => _message = e.message);
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        if (_balance != null) Text('Kalan bakiye: ${formatTry(_balance!)}', style: const TextStyle(color: navy)),
        TextField(
          controller: _recipient,
          keyboardType: TextInputType.emailAddress,
          decoration: const InputDecoration(labelText: 'Alıcı e-posta'),
        ),
        TextField(
          controller: _amount,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: const InputDecoration(labelText: 'Tutar'),
        ),
        TextField(
          controller: _description,
          decoration: const InputDecoration(labelText: 'Açıklama'),
        ),
        const SizedBox(height: 16),
        FilledButton(
          onPressed: _busy ? null : _send,
          child: Text(_busy ? '…' : 'Gönder'),
        ),
        if (_message != null) ...[
          const SizedBox(height: 12),
          Text(_message!),
        ],
      ],
    );
  }
}
