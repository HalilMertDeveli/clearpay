import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../theme.dart';
import 'receipt_screen.dart';

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

  void _openReceipt(String correlationId) {
    if (correlationId.isEmpty) {
      return;
    }
    Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => ReceiptScreen(api: widget.api, correlationId: correlationId),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: wash,
      child: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
          children: [
            if (_error != null)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(_error!, style: const TextStyle(color: Colors.red)),
              ),
            if (_wallet == null && _error == null)
              const Padding(
                padding: EdgeInsets.only(top: 48),
                child: Center(child: CircularProgressIndicator()),
              ),
            if (_wallet != null) ...[
              _BalanceCard(wallet: _wallet!),
              const SizedBox(height: 16),
              const Text(
                'Hızlı işlemler',
                style: TextStyle(fontWeight: FontWeight.w600, color: navy),
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  _QuickTile(
                    icon: Icons.swap_horiz,
                    label: 'Havale',
                    onTap: () => widget.onOpenTab?.call(1),
                  ),
                  _QuickTile(
                    icon: Icons.add_card_outlined,
                    label: 'Yükle',
                    onTap: () => widget.onOpenTab?.call(2),
                  ),
                  _QuickTile(
                    icon: Icons.south_west,
                    label: 'Çek',
                    onTap: () => widget.onOpenTab?.call(2),
                  ),
                  _QuickTile(
                    icon: Icons.receipt_long_outlined,
                    label: 'Hareketler',
                    onTap: () => widget.onOpenTab?.call(3),
                  ),
                ],
              ),
              const SizedBox(height: 20),
              Row(
                children: [
                  const Expanded(
                    child: Text(
                      'Son hareketler',
                      style: TextStyle(fontWeight: FontWeight.w600, color: navy),
                    ),
                  ),
                  TextButton(
                    onPressed: () => widget.onOpenTab?.call(3),
                    child: const Text('Tümü'),
                  ),
                ],
              ),
              if (_wallet!.lastMovements.isEmpty)
                const Padding(
                  padding: EdgeInsets.only(top: 8),
                  child: Text('Henüz hareket yok.', style: TextStyle(color: muted)),
                ),
              for (final row in _wallet!.lastMovements)
                Material(
                  color: Colors.white,
                  child: ListTile(
                    title: Text(row.kind),
                    subtitle: Text(row.at, style: const TextStyle(color: muted)),
                    trailing: Text(
                      formatTry(row.amount),
                      style: const TextStyle(fontWeight: FontWeight.w600, color: navy),
                    ),
                    onTap: () => _openReceipt(row.correlationId),
                  ),
                ),
            ],
          ],
        ),
      ),
    );
  }
}

class _BalanceCard extends StatelessWidget {
  const _BalanceCard({required this.wallet});

  final WalletSnapshot wallet;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(20, 20, 20, 18),
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [navyHero, navy],
        ),
        borderRadius: BorderRadius.all(Radius.circular(12)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Cüzdan',
            style: TextStyle(color: Colors.white70, fontSize: 13, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          Text(
            formatTry(wallet.balance),
            style: const TextStyle(
              color: Colors.white,
              fontSize: 32,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: 12),
          Text(
            'Bu ay giden ${formatTry(wallet.monthOutgoing)}  ·  gelen ${formatTry(wallet.monthIncoming)}',
            style: const TextStyle(color: Colors.white70, fontSize: 13),
          ),
          if (wallet.isFrozen) ...[
            const SizedBox(height: 10),
            const Text(
              'Cüzdan dondurulmuş',
              style: TextStyle(color: Color(0xFFFFC9C9), fontWeight: FontWeight.w600),
            ),
          ],
        ],
      ),
    );
  }
}

class _QuickTile extends StatelessWidget {
  const _QuickTile({required this.icon, required this.label, required this.onTap});

  final IconData icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 4),
        child: Material(
          color: Colors.white,
          shape: const RoundedRectangleBorder(
            borderRadius: BorderRadius.all(Radius.circular(12)),
            side: BorderSide(color: line),
          ),
          child: InkWell(
            onTap: onTap,
            borderRadius: const BorderRadius.all(Radius.circular(12)),
            child: Padding(
              padding: const EdgeInsets.symmetric(vertical: 14),
              child: Column(
                children: [
                  Icon(icon, color: teal),
                  const SizedBox(height: 6),
                  FittedBox(
                    fit: BoxFit.scaleDown,
                    child: Text(
                      label,
                      textAlign: TextAlign.center,
                      style: const TextStyle(fontSize: 12, color: navy, fontWeight: FontWeight.w600),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
