import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';
import 'receipt_screen.dart';

class MovementsScreen extends StatefulWidget {
  const MovementsScreen({super.key, required this.api, this.liveTick = 0});

  final ClearPayClient api;
  final int liveTick;

  @override
  State<MovementsScreen> createState() => _MovementsScreenState();
}

class _MovementsScreenState extends State<MovementsScreen> {
  MovementPage _page = MovementPage(items: const [], page: 1, pageSize: 20, totalCount: 0);
  String? _error;
  String _kind = 'all';
  DateTime? _from;
  DateTime? _to;
  int _pageIndex = 1;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void didUpdateWidget(MovementsScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.liveTick != widget.liveTick) {
      _load();
    }
  }

  String? _iso(DateTime? value) =>
      value == null ? null : '${value.year.toString().padLeft(4, '0')}-${value.month.toString().padLeft(2, '0')}-${value.day.toString().padLeft(2, '0')}';

  Future<void> _load() async {
    try {
      final page = await widget.api.movements(
        kind: _kind,
        from: _iso(_from),
        to: _iso(_to),
        page: _pageIndex,
      );
      if (!mounted) {
        return;
      }
      setState(() {
        _page = page;
        _error = null;
      });
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _error = e.message);
    }
  }

  Future<void> _pickFrom() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _from ?? DateTime.now(),
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(days: 1)),
    );
    if (picked == null) {
      return;
    }
    setState(() {
      _from = picked;
      _pageIndex = 1;
    });
  }

  Future<void> _pickTo() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _to ?? DateTime.now(),
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(days: 1)),
    );
    if (picked == null) {
      return;
    }
    setState(() {
      _to = picked;
      _pageIndex = 1;
    });
  }

  @override
  Widget build(BuildContext context) {
    final l = l10n(context);
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
            child: Column(
              children: [
                Row(
                  children: [
                    Expanded(
                      child: DropdownButton<String>(
                        isExpanded: true,
                        value: _kind,
                        items: [
                          DropdownMenuItem(value: 'all', child: Text(l.filterAll)),
                          DropdownMenuItem(value: 'Transfer', child: Text(l.transfer)),
                          DropdownMenuItem(value: 'TopUp', child: Text(l.topUpKind)),
                          DropdownMenuItem(value: 'Withdraw', child: Text(l.withdrawKind)),
                        ],
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }
                          setState(() {
                            _kind = value;
                            _pageIndex = 1;
                          });
                        },
                      ),
                    ),
                    TextButton(onPressed: _load, child: Text(l.filter)),
                  ],
                ),
                Row(
                  children: [
                    TextButton(
                      onPressed: _pickFrom,
                      child: Text(_from == null ? l.filterFrom : _iso(_from)!),
                    ),
                    TextButton(
                      onPressed: _pickTo,
                      child: Text(_to == null ? l.filterTo : _iso(_to)!),
                    ),
                    if (_from != null || _to != null)
                      TextButton(
                        onPressed: () {
                          setState(() {
                            _from = null;
                            _to = null;
                            _pageIndex = 1;
                          });
                          _load();
                        },
                        child: Text(l.clear),
                      ),
                  ],
                ),
              ],
            ),
          ),
          if (_error != null)
            Padding(
              padding: const EdgeInsets.all(16),
              child: Text(_error!, style: const TextStyle(color: Colors.red)),
            ),
          if (_page.items.isEmpty && _error == null)
            Padding(
              padding: const EdgeInsets.all(16),
              child: Text(l.emptyPeriod, style: const TextStyle(color: muted)),
            ),
          for (final row in _page.items)
            ListTile(
              title: Text('${row.kind} · ${l.money(row.signedAmount)}'),
              subtitle: Text('${row.counterparty}\n${row.at}'),
              isThreeLine: true,
              trailing: Text(l.receipt),
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
          if (_page.totalCount > _page.pageSize)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 8),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  TextButton(
                    onPressed: _pageIndex <= 1
                        ? null
                        : () {
                            setState(() => _pageIndex--);
                            _load();
                          },
                    child: Text(l.previous),
                  ),
                  Text('${_page.page} / ${_page.totalPages}'),
                  TextButton(
                    onPressed: _pageIndex >= _page.totalPages
                        ? null
                        : () {
                            setState(() => _pageIndex++);
                            _load();
                          },
                    child: Text(l.next),
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }
}
