import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../api/clearpay_client.dart';
import '../l10n/locale_scope.dart';
import '../platform/receipt_pdf.dart';
import '../theme.dart';

Future<void> openReceipt(
  BuildContext context,
  ClearPayClient api,
  String correlationId,
) async {
  if (correlationId.isEmpty) {
    return;
  }
  await Navigator.of(context).push(
    MaterialPageRoute<void>(
      builder: (_) => ReceiptScreen(api: api, correlationId: correlationId),
    ),
  );
}

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
  bool _pdfBusy = false;

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

  Future<void> _copyRef() async {
    await Clipboard.setData(ClipboardData(text: widget.correlationId));
    if (!mounted) {
      return;
    }
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(l10n(context).copied)),
    );
  }

  Future<void> _openPdf() async {
    setState(() {
      _pdfBusy = true;
      _error = null;
    });
    try {
      final bytes = await widget.api.receiptPdf(widget.correlationId);
      await openReceiptPdf(widget.correlationId, bytes);
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _error = e.message);
    } catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _error = e.toString());
    } finally {
      if (mounted) {
        setState(() => _pdfBusy = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = l10n(context);
    final receipt = _receipt;
    return Scaffold(
      appBar: AppBar(title: Text(l.receipt)),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
          if (receipt != null) ...[
            Text(l.money(receipt.amount), style: const TextStyle(fontSize: 28, color: navy)),
            const SizedBox(height: 12),
            Text('${l.kindLabel}: ${receipt.kind}'),
            Text('${l.debitParty}: ${receipt.debitParty}'),
            Text('${l.creditParty}: ${receipt.creditParty}'),
            Text('${l.time}: ${receipt.at}'),
            Text('${l.correlation}: ${receipt.correlationId}'),
            if (receipt.instrumentHint != null) Text('${l.accountCard}: ${receipt.instrumentHint}'),
            if (receipt.description != null) Text(receipt.description!),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              children: [
                OutlinedButton(onPressed: _copyRef, child: Text(l.copy)),
                FilledButton(
                  onPressed: _pdfBusy ? null : _openPdf,
                  child: Text(_pdfBusy ? '…' : l.downloadPdf),
                ),
              ],
            ),
          ],
          const DemoFooter(),
        ],
      ),
    );
  }
}
