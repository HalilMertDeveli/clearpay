import 'package:flutter/material.dart';

import '../api/clearpay_client.dart';
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
      setState(() => _message = freeze ? 'Donduruldu.' : 'Çözüldü.');
    } on ApiException catch (e) {
      setState(() => _message = e.message);
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        const Text('Cüzdan dondur', style: TextStyle(fontWeight: FontWeight.w600, color: navy)),
        TextField(controller: _email, decoration: const InputDecoration(labelText: 'E-posta')),
        Row(
          children: [
            FilledButton(onPressed: () => _toggle(true), child: const Text('Dondur')),
            const SizedBox(width: 8),
            OutlinedButton(onPressed: () => _toggle(false), child: const Text('Çöz')),
          ],
        ),
        const SizedBox(height: 16),
        const Text('Başarısız kuyruk', style: TextStyle(fontWeight: FontWeight.w600)),
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
              child: const Text('Kuyruğa al'),
            ),
          ),
        const SizedBox(height: 16),
        const Text('Audit', style: TextStyle(fontWeight: FontWeight.w600)),
        TextField(controller: _actor, decoration: const InputDecoration(labelText: 'Aktör ara')),
        TextField(
          controller: _correlation,
          decoration: const InputDecoration(labelText: 'Correlation id'),
        ),
        TextButton(onPressed: _reload, child: const Text('Ara')),
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
