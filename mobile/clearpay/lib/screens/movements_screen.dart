import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../theme.dart';
import 'receipt_screen.dart';

class MovementsScreen extends StatefulWidget {
  const MovementsScreen({super.key, required this.api});

  final ClearPayClient api;

  @override
  State<MovementsScreen> createState() => _MovementsScreenState();
}

class _MovementsScreenState extends State<MovementsScreen> {
  List<MovementRow> _items = [];
  String? _error;
  String _kind = 'all';

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final items = await widget.api.movements(kind: _kind);
      if (!mounted) {
        return;
      }
      setState(() {
        _items = items;
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
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
            child: Row(
              children: [
                Expanded(
                  child: DropdownButton<String>(
                    isExpanded: true,
                    value: _kind,
                    items: const [
                      DropdownMenuItem(value: 'all', child: Text('Tümü')),
                      DropdownMenuItem(value: 'Transfer', child: Text('Havale')),
                      DropdownMenuItem(value: 'TopUp', child: Text('Yükleme')),
                      DropdownMenuItem(value: 'Withdraw', child: Text('Çekim')),
                    ],
                    onChanged: (value) {
                      if (value == null) {
                        return;
                      }
                      setState(() => _kind = value);
                    },
                  ),
                ),
                TextButton(onPressed: _load, child: const Text('Filtrele')),
              ],
            ),
          ),
          if (_error != null)
            Padding(
              padding: const EdgeInsets.all(16),
              child: Text(_error!, style: const TextStyle(color: Colors.red)),
            ),
          for (final row in _items)
            ListTile(
              title: Text('${row.kind} · ${formatTry(row.signedAmount)}'),
              subtitle: Text('${row.counterparty}\n${row.at}'),
              isThreeLine: true,
              trailing: const Text('Dekont'),
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => ReceiptScreen(
                      api: widget.api,
                      correlationId: row.correlationId,
                    ),
                  ),
                );
              },
            ),
        ],
      ),
    );
  }
}
