class PayUri {
  const PayUri({required this.to, this.amount});

  final String to;
  final String? amount;

  String encode() {
    final amountQuery =
        amount == null || amount!.isEmpty ? '' : '&amount=${Uri.encodeQueryComponent(amount!)}';
    return 'clearpay://pay?to=${Uri.encodeQueryComponent(to)}$amountQuery';
  }

  static PayUri? tryParse(String raw) {
    final text = raw.trim();
    if (text.isEmpty) {
      return null;
    }
    if (text.contains('@') && !text.contains('://') && !text.contains('to=')) {
      return PayUri(to: text);
    }
    final uri = Uri.tryParse(text);
    if (uri == null) {
      return null;
    }
    final to = uri.queryParameters['to'] ?? uri.queryParameters['email'];
    if (to == null || to.isEmpty) {
      return text.contains('@') ? PayUri(to: text) : null;
    }
    final amount = uri.queryParameters['amount'];
    return PayUri(to: to, amount: amount == null || amount.isEmpty ? null : amount);
  }
}
