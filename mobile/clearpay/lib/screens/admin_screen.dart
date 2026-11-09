import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
import '../l10n/locale_scope.dart';
import '../theme.dart';

class AdminScreen extends StatefulWidget {
  const AdminScreen({super.key, required this.api, this.liveTick = 0});

  final ClearPayClient api;
  final int liveTick;

  @override
  State<AdminScreen> createState() => _AdminScreenState();
}

class _AdminScreenState extends State<AdminScreen> {
  final _email = TextEditingController();
  final _actor = TextEditingController();
  final _correlation = TextEditingController();
  List<OutboxRow> _outbox = [];
  List<AuditRow> _audits = [];
  String? _message;

  @override
  void initState() {
    super.initState();
    _reload();
  }

  @override
  void didUpdateWidget(AdminScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.liveTick != widget.liveTick) {
      _reload();
    }
  }

  @override
  void dispose() {
    _email.dispose();
    _actor.dispose();
    _correlation.dispose();
    super.dispose();
  }

  Future<void> _reload() async {
    try {
      final outbox = await widget.api.failedOutbox();
      final audits = await widget.api.audit(actor: _actor.text, correlationId: _correlation.text);
      if (!mounted) {
        return;
      }
      setState(() {
        _outbox = outbox;
        _audits = audits;
        _message = null;
      });
    } on ApiException catch (e) {
      if (!mounted) {
        return;
      }
      setState(() => _message = e.message);
    }
  }

  Future<void> _toggle(bool freeze) async {
    try {
      if (freeze) {
        await widget.api.freeze(_email.text);
      } else {
        await widget.api.unfreeze(_email.text);
      }
      setState(() => _message = freeze ? l10n(context).frozenMsg : l10n(context).unfrozenMsg);
    } on ApiException catch (e) {
      setState(() => _message = e.message);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = l10n(context);
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        Text(l.freezeWallet, style: const TextStyle(fontWeight: FontWeight.w600, color: navy)),
        TextField(controller: _email, decoration: InputDecoration(labelText: l.email)),
        Row(
          children: [
            FilledButton(onPressed: () => _toggle(true), child: Text(l.freeze)),
            const SizedBox(width: 8),
            OutlinedButton(onPressed: () => _toggle(false), child: Text(l.unfreeze)),
          ],
        ),
        const SizedBox(height: 16),
        Text(l.failedQueue, style: const TextStyle(fontWeight: FontWeight.w600)),
        for (final row in _outbox)
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(row.type),
            subtitle: Text(row.correlationId),
            trailing: TextButton(
              onPressed: () async {
                await widget.api.requeue(row.id);
                await _reload();
              },
              child: Text(l.requeue),
            ),
          ),
        const SizedBox(height: 16),
        Text(l.audit, style: const TextStyle(fontWeight: FontWeight.w600)),
        TextField(controller: _actor, decoration: InputDecoration(labelText: l.searchActor)),
        TextField(
          controller: _correlation,
          decoration: InputDecoration(labelText: l.correlationId),
        ),
        TextButton(onPressed: _reload, child: Text(l.search)),
        for (final row in _audits)
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(row.action),
            subtitle: Text('${row.actorUserId}\n${row.createdAt}'),
            isThreeLine: true,
          ),
        if (_message != null) Text(_message!),
      ],
    );
  }
}
