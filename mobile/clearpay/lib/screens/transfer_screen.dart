import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../qr/pay_uri.dart';
import '../theme.dart';
import 'receipt_screen.dart';

class TransferPrefill {
  const TransferPrefill({this.recipient, this.amount, this.description});

  final String? recipient;
  final String? amount;
  final String? description;
}

class TransferScreen extends StatefulWidget {
  const TransferScreen({super.key, required this.api, this.prefill});

  final ClearPayClient api;
  final TransferPrefill? prefill;

  @override
  State<TransferScreen> createState() => _TransferScreenState();
}

class _TransferScreenState extends State<TransferScreen> {
  late final TextEditingController _recipient;
  late final TextEditingController _amount;
  late final TextEditingController _description;
  double? _balance;
  bool _frozen = false;
  String? _message;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _recipient = TextEditingController(text: widget.prefill?.recipient ?? '');
    _amount = TextEditingController(text: widget.prefill?.amount ?? '');
    _description = TextEditingController(text: widget.prefill?.description ?? 'demo');
    _loadBalance();
  }

  @override
  void didUpdateWidget(covariant TransferScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    final next = widget.prefill;
    if (next != null && !identical(next, oldWidget.prefill)) {
      if (next.recipient != null) {
        _recipient.text = next.recipient!;
      }
      if (next.amount != null) {
        _amount.text = next.amount!;
      }
    }
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
      setState(() {
        _balance = wallet.balance;
        _frozen = wallet.isFrozen;
        if (wallet.isFrozen) {
          _message = 'Cüzdan dondurulmuş; gönderim kapalı.';
        }
      });
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _message = e.message);
    }
  }

  Future<void> _pasteQr() async {
    final field = TextEditingController();
    final raw = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('QR yapıştır'),
        content: TextField(
          controller: field,
          decoration: const InputDecoration(labelText: 'clearpay://pay?to=… veya e-posta'),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('İptal')),
          FilledButton(onPressed: () => Navigator.pop(ctx, field.text), child: const Text('Doldur')),
        ],
      ),
    );
    if (raw == null) {
      return;
    }
    final parsed = PayUri.tryParse(raw);
    if (parsed == null) {
      setState(() => _message = 'Geçerli ClearPay QR veya e-posta girin.');
      return;
    }
    setState(() {
      _recipient.text = parsed.to;
      if (parsed.amount != null) {
        _amount.text = parsed.amount!;
      }
      _message = 'QR alıcı forma yazıldı. Onay + POST /api/transfers.';
    });
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
      final posted = await widget.api.transfer(
        recipient: _recipient.text,
        amount: amount,
        description: _description.text,
      );
      if (!mounted) {
        return;
      }
      await openReceipt(context, widget.api, posted.correlationId);
    } on ApiException catch (e) {
      if (e.status == 409 && e.correlationId != null && e.correlationId!.isNotEmpty) {
        if (!mounted) {
          return;
        }
        await openReceipt(context, widget.api, e.correlationId!);
        return;
      }
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
        const Text(
          'QR ile öde bu formdur. Jet QR değil.',
          style: TextStyle(color: muted, fontSize: 12),
        ),
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
        const SizedBox(height: 8),
        OutlinedButton(onPressed: _pasteQr, child: const Text('QR yapıştır')),
        const SizedBox(height: 8),
        FilledButton(
          onPressed: (_busy || _frozen) ? null : _send,
          child: Text(_frozen ? 'Dondurulmuş' : (_busy ? '…' : 'Gönder')),
        ),
        if (_message != null) ...[
          const SizedBox(height: 12),
          Text(_message!),
        ],
      ],
    );
  }
}
