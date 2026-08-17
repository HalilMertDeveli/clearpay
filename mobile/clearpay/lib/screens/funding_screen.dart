import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';
import 'receipt_screen.dart';

class FundingScreen extends StatefulWidget {
  const FundingScreen({super.key, required this.api, this.liveTick = 0});

  final ClearPayClient api;
  final int liveTick;

  @override
  State<FundingScreen> createState() => _FundingScreenState();
}

class _FundingScreenState extends State<FundingScreen> {
  final _amount = TextEditingController();
  final _account = TextEditingController(text: '****1234');
  final _last4 = TextEditingController();
  final _label = TextEditingController(text: 'Demo kart');
  List<CardSnapshot> _cards = [];
  String? _message;
  bool _busy = false;
  bool _frozen = false;

  @override
  void initState() {
    super.initState();
    _loadCards();
  }

  @override
  void didUpdateWidget(FundingScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.liveTick != widget.liveTick) {
      _loadCards();
    }
  }

  @override
  void dispose() {
    _amount.dispose();
    _account.dispose();
    _last4.dispose();
    _label.dispose();
    super.dispose();
  }

  Future<void> _loadCards() async {
    try {
      final cards = await widget.api.cards();
      final wallet = await widget.api.wallet();
      if (!mounted) {
        return;
      }
      setState(() {
        _cards = cards;
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

  Future<void> _addCard() async {
    try {
      await widget.api.addCard(last4: _last4.text, label: _label.text);
      _last4.clear();
      await _loadCards();
      setState(() => _message = l10n(context).cardAdded);
    } on ApiException catch (e) {
      setState(() => _message = e.message);
    }
  }

  Future<void> _run(Future<PostedMoney> Function({required double amount, required String account}) action) async {
    final amount = double.tryParse(_amount.text.replaceAll(',', '.'));
    if (amount == null || amount <= 0) {
      setState(() => _message = l10n(context).validAmount);
      return;
    }
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      final posted = await action(amount: amount, account: _account.text);
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
        Text(l.linkedCard, style: const TextStyle(fontWeight: FontWeight.w600)),
        if (_frozen) Text(l.frozenNoFunding, style: const TextStyle(color: navy)),
        for (final card in _cards)
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text('${card.label} · ${card.accountHint}'),
            onTap: () => setState(() => _account.text = card.accountHint),
          ),
        TextField(controller: _last4, decoration: InputDecoration(labelText: l.last4)),
        TextField(controller: _label, decoration: InputDecoration(labelText: l.cardLabel)),
        TextButton(onPressed: _addCard, child: Text(l.addCard)),
        const Divider(),
        TextField(
          controller: _amount,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(labelText: l.amount),
        ),
        TextField(
          controller: _account,
          decoration: InputDecoration(labelText: l.accountHint),
        ),
        const SizedBox(height: 16),
        FilledButton(
          onPressed: (_busy || _frozen) ? null : () => _run(widget.api.topUp),
          child: Text(_frozen ? l.frozen : l.topUp),
        ),
        const SizedBox(height: 8),
        OutlinedButton(
          onPressed: (_busy || _frozen) ? null : () => _run(widget.api.withdraw),
          child: Text(l.withdraw),
        ),
        if (_message != null) ...[
          const SizedBox(height: 12),
          Text(_message!, style: const TextStyle(color: navy)),
        ],
      ],
    );
  }
}
