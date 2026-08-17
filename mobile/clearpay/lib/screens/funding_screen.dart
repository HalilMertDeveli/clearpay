import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../theme.dart';

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
          _message = 'Cüzdan dondurulmuş; yükle/çek kapalı.';
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
      setState(() => _message = 'Kart eklendi (PAN yok, yalnız son 4).');
    } on ApiException catch (e) {
      setState(() => _message = e.message);
    }
  }

  Future<void> _run(Future<void> Function({required double amount, required String account}) action) async {
    final amount = double.tryParse(_amount.text.replaceAll(',', '.'));
    if (amount == null || amount <= 0) {
      setState(() => _message = 'Geçerli tutar girin.');
      return;
    }
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      await action(amount: amount, account: _account.text);
      setState(() => _message = 'Tamam. Özetten bakiyeyi yenileyin.');
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
        const Text('Kayıtlı kart (demo, PAN yok)', style: TextStyle(fontWeight: FontWeight.w600)),
        for (final card in _cards)
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text('${card.label} · ${card.accountHint}'),
            onTap: () => setState(() => _account.text = card.accountHint),
          ),
        TextField(controller: _last4, decoration: const InputDecoration(labelText: 'Son 4 hane')),
        TextField(controller: _label, decoration: const InputDecoration(labelText: 'Etiket')),
        TextButton(onPressed: _addCard, child: const Text('Kart ekle')),
        const Divider(),
        TextField(
          controller: _amount,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: const InputDecoration(labelText: 'Tutar'),
        ),
        TextField(
          controller: _account,
          decoration: const InputDecoration(labelText: 'Hesap ipucu (TIMEOUT = zaman aşımı)'),
        ),
        const SizedBox(height: 16),
        FilledButton(
          onPressed: (_busy || _frozen) ? null : () => _run(widget.api.topUp),
          child: Text(_frozen ? 'Dondurulmuş' : 'Yükle'),
        ),
        const SizedBox(height: 8),
        OutlinedButton(
          onPressed: (_busy || _frozen) ? null : () => _run(widget.api.withdraw),
          child: const Text('Çek'),
        ),
        if (_message != null) ...[
          const SizedBox(height: 12),
          Text(_message!, style: const TextStyle(color: navy)),
        ],
      ],
    );
  }
}
