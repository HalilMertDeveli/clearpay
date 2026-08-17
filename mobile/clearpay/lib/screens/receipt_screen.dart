import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../theme.dart';

class ReceiptScreen extends StatefulWidget {
  const ReceiptScreen({
    super.key,
    required this.api,
    required this.correlationId,
  });

  final ClearPayClient api;
  final String correlationId;

  @override
  State<ReceiptScreen> createState() => _ReceiptScreenState();
}

class _ReceiptScreenState extends State<ReceiptScreen> {
  ReceiptSnapshot? _receipt;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final receipt = await widget.api.receipt(widget.correlationId);
      if (!mounted) {
        return;
      }
      setState(() => _receipt = receipt);
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _error = e.message);
    }
  }

  @override
  Widget build(BuildContext context) {
    final receipt = _receipt;
    return Scaffold(
      appBar: AppBar(title: const Text('Dekont')),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
          if (receipt != null) ...[
            Text(formatTry(receipt.amount), style: const TextStyle(fontSize: 28, color: navy)),
            const SizedBox(height: 12),
            Text('Tür: ${receipt.kind}'),
            Text('Borç: ${receipt.debitParty}'),
            Text('Alacak: ${receipt.creditParty}'),
            Text('Zaman: ${receipt.at}'),
            Text('Correlation: ${receipt.correlationId}'),
            if (receipt.description != null) Text(receipt.description!),
          ],
          const DemoFooter(),
        ],
      ),
    );
  }
}
