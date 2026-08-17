import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../l10n/locale_scope.dart';
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
          _message = null;
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
    final l = l10n(context);
    final field = TextEditingController();
    final raw = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(l.pasteQr),
        content: TextField(
          controller: field,
          decoration: InputDecoration(labelText: l.qrOrEmail),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: Text(l.cancel)),
          FilledButton(onPressed: () => Navigator.pop(ctx, field.text), child: Text(l.fill)),
        ],
      ),
    );
    if (raw == null) {
      return;
    }
    final parsed = PayUri.tryParse(raw);
    if (parsed == null) {
      setState(() => _message = l.invalidQr);
      return;
    }
    setState(() {
      _recipient.text = parsed.to;
      if (parsed.amount != null) {
        _amount.text = parsed.amount!;
      }
      _message = l.qrFilledForm;
    });
  }

  Future<void> _send() async {
    final l = l10n(context);
    final amount = double.tryParse(_amount.text.replaceAll(',', '.'));
    if (amount == null || amount <= 0) {
      setState(() => _message = l.validAmount);
      return;
    }
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(l.confirmTransfer),
        content: Text(l.confirmSend(_recipient.text, l.money(amount))),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(l.cancel)),
          FilledButton(onPressed: () => Navigator.pop(ctx, true), child: Text(l.send)),
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
    final l = l10n(context);
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        if (_balance != null) Text(l.remainingBalance(l.money(_balance!)), style: const TextStyle(color: navy)),
        if (_frozen) Text(l.frozenNoSend, style: const TextStyle(color: navy)),
        Text(
          l.payQrIsThisForm,
          style: const TextStyle(color: muted, fontSize: 12),
        ),
        TextField(
          controller: _recipient,
          keyboardType: TextInputType.emailAddress,
          decoration: InputDecoration(labelText: l.recipientEmail),
        ),
        TextField(
          controller: _amount,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(labelText: l.amount),
        ),
        TextField(
          controller: _description,
          decoration: InputDecoration(labelText: l.description),
        ),
        const SizedBox(height: 8),
        OutlinedButton(onPressed: _pasteQr, child: Text(l.pasteQr)),
        const SizedBox(height: 8),
        FilledButton(
          onPressed: (_busy || _frozen) ? null : _send,
          child: Text(_frozen ? l.frozen : (_busy ? '…' : l.send)),
        ),
        if (_message != null) ...[
          const SizedBox(height: 12),
          Text(_message!),
        ],
      ],
    );
  }
}
