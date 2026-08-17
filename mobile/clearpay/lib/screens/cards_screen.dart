import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../api/clearpay_client.dart';
import '../cards/card_network.dart';
import '../cards/live_payment_card.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';

class CardsScreen extends StatefulWidget {
  const CardsScreen({super.key, required this.api, this.liveTick = 0, this.onLoadFromCard});

  final ClearPayClient api;
  final int liveTick;
  final ValueChanged<String>? onLoadFromCard;

  @override
  State<CardsScreen> createState() => _CardsScreenState();
}

class _CardsScreenState extends State<CardsScreen> {
  final _number = TextEditingController();
  final _holder = TextEditingController();
  final _expiry = TextEditingController();
  final _cvv = TextEditingController();
  final _label = TextEditingController();
  List<CardSnapshot> _cards = [];
  String? _message;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _number.addListener(_onPreview);
    _holder.addListener(_onPreview);
    _expiry.addListener(_onPreview);
    _label.addListener(_onPreview);
    _load();
  }

  @override
  void didUpdateWidget(CardsScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.liveTick != widget.liveTick) {
      _load();
    }
  }

  @override
  void dispose() {
    _number.dispose();
    _holder.dispose();
    _expiry.dispose();
    _cvv.dispose();
    _label.dispose();
    super.dispose();
  }

  void _onPreview() {
    if (mounted) {
      setState(() {});
    }
  }

  Future<void> _load() async {
    try {
      final cards = await widget.api.cards();
      if (!mounted) {
        return;
      }
      setState(() => _cards = cards);
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _message = e.message);
    }
  }

  Future<void> _add() async {
    setState(() {
      _busy = true;
      _message = null;
    });
    try {
      await widget.api.addCard(
        number: _number.text,
        label: _label.text.trim().isEmpty ? _holder.text : _label.text,
      );
      _number.clear();
      _holder.clear();
      _expiry.clear();
      _cvv.clear();
      await _load();
      if (!mounted) {
        return;
      }
      setState(() => _message = l10n(context).cardAdded);
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
    final l = l10n(context);
    final digits = _number.text.replaceAll(RegExp(r'\D'), '');
    final nickname = _label.text.trim().isEmpty ? _holder.text : _label.text;
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        Text(l.cardsLede, style: const TextStyle(color: muted)),
        const SizedBox(height: 16),
        LivePaymentCard(
          digits: digits,
          holder: nickname,
          expiry: _expiry.text,
        ),
        const SizedBox(height: 16),
        TextField(
          controller: _number,
          keyboardType: TextInputType.number,
          inputFormatters: [
            FilteringTextInputFormatter.digitsOnly,
            _CardNumberFormatter(),
            LengthLimitingTextInputFormatter(19),
          ],
          decoration: InputDecoration(
            labelText: l.cardNumber,
            hintText: '4111 1111 1111 1111',
          ),
        ),
        TextField(
          controller: _holder,
          textCapitalization: TextCapitalization.characters,
          decoration: InputDecoration(labelText: l.cardHolder),
        ),
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: _expiry,
                keyboardType: TextInputType.number,
                inputFormatters: [
                  FilteringTextInputFormatter.digitsOnly,
                  _ExpiryFormatter(),
                  LengthLimitingTextInputFormatter(5),
                ],
                decoration: InputDecoration(labelText: l.cardExpiry, hintText: '12/28'),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: TextField(
                controller: _cvv,
                keyboardType: TextInputType.number,
                obscureText: true,
                inputFormatters: [
                  FilteringTextInputFormatter.digitsOnly,
                  LengthLimitingTextInputFormatter(4),
                ],
                decoration: InputDecoration(labelText: l.cardCvv),
              ),
            ),
          ],
        ),
        TextField(
          controller: _label,
          decoration: InputDecoration(labelText: l.cardNickname, hintText: 'Yapı Kredi'),
        ),
        const SizedBox(height: 12),
        FilledButton(
          onPressed: _busy ? null : _add,
          child: Text(l.addCard),
        ),
        if (_message != null) ...[
          const SizedBox(height: 12),
          Text(_message!, style: const TextStyle(color: navy)),
        ],
        const SizedBox(height: 24),
        Text(l.linkedCard, style: const TextStyle(fontWeight: FontWeight.w600, color: navy)),
        if (_cards.isEmpty)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(l.noLinkedCard, style: const TextStyle(color: muted)),
          ),
        for (final card in _cards) ...[
          const SizedBox(height: 12),
          LivePaymentCard(
            digits: '',
            last4: card.last4,
            holder: card.label,
            scheme: CardNetwork.parseStored(card.scheme),
          ),
          Align(
            alignment: Alignment.centerLeft,
            child: TextButton(
              onPressed: () => widget.onLoadFromCard?.call(card.accountHint),
              child: Text(l.loadFromCard),
            ),
          ),
        ],
      ],
    );
  }
}

class _CardNumberFormatter extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final digits = newValue.text.replaceAll(RegExp(r'\D'), '');
    final grouped = digits.replaceAllMapped(RegExp(r'.{1,4}'), (m) => '${m.group(0)} ').trim();
    return TextEditingValue(
      text: grouped,
      selection: TextSelection.collapsed(offset: grouped.length),
    );
  }
}

class _ExpiryFormatter extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final digits = newValue.text.replaceAll(RegExp(r'\D'), '');
    final text = digits.length <= 2 ? digits : '${digits.substring(0, 2)}/${digits.substring(2)}';
    return TextEditingValue(
      text: text,
      selection: TextSelection.collapsed(offset: text.length),
    );
  }
}
