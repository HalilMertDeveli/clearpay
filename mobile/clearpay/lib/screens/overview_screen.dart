import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../theme.dart';

class OverviewScreen extends StatefulWidget {
  const OverviewScreen({super.key, required this.api, this.onOpenTab});

  final ClearPayClient api;
  final ValueChanged<int>? onOpenTab;

  @override
  State<OverviewScreen> createState() => _OverviewScreenState();
}

class _OverviewScreenState extends State<OverviewScreen> {
  WalletSnapshot? _wallet;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final wallet = await widget.api.wallet();
      if (!mounted) {
        return;
      }
      setState(() {
        _wallet = wallet;
        _error = null;
      });
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _error = e.message);
    }
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(20),
        children: [
          if (_error != null) Text(_error!, style: const TextStyle(color: Colors.red)),
          if (_wallet != null) ...[
            Text(formatTry(_wallet!.balance), style: const TextStyle(fontSize: 32, color: navy)),
            const SizedBox(height: 8),
            Text('Bu ay giden: ${formatTry(_wallet!.monthOutgoing)}'),
            Text('Bu ay gelen: ${formatTry(_wallet!.monthIncoming)}'),
            if (_wallet!.isFrozen)
              const Padding(
                padding: EdgeInsets.only(top: 8),
                child: Text('Cüzdan dondurulmuş', style: TextStyle(color: Colors.red)),
              ),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              children: [
                FilledButton(
                  onPressed: () => widget.onOpenTab?.call(1),
                  child: const Text('Havale gönder'),
                ),
                OutlinedButton(
                  onPressed: () => widget.onOpenTab?.call(2),
                  child: const Text('Yükle'),
                ),
                OutlinedButton(
                  onPressed: () => widget.onOpenTab?.call(2),
                  child: const Text('Çek'),
                ),
              ],
            ),
            const SizedBox(height: 16),
            const Text('Son hareketler', style: TextStyle(fontWeight: FontWeight.w600)),
            for (final row in _wallet!.lastMovements)
              ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text(row.kind),
                subtitle: Text(row.at),
                trailing: Text(formatTry(row.amount)),
              ),
          ],
        ],
      ),
    );
  }
}
