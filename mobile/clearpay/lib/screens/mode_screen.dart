import 'package:flutter/material.dart';

import '../auth/account_kind_store.dart';
import '../theme.dart';

class ModeScreen extends StatelessWidget {
  const ModeScreen({super.key, required this.store, required this.onPicked});

  final AccountKindStore store;
  final ValueChanged<String> onPicked;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('ClearPay')),
      body: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          const Text(
            'Nasıl devam edilsin?',
            style: TextStyle(color: navy, fontSize: 22, fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 8),
          const Text(
            'Aynı 8 işlem, aynı SQL defteri. Kurumsal üye iş yeri / POS değil.',
            style: TextStyle(color: muted),
          ),
          const SizedBox(height: 20),
          _ModeCard(
            title: bireyselKind,
            subtitle: 'Kişisel cüzdan görünümü.',
            icon: Icons.person_outline,
            selected: store.kind == bireyselKind,
            onTap: () => onPicked(bireyselKind),
          ),
          const SizedBox(height: 12),
          _ModeCard(
            title: kurumsalKind,
            subtitle: 'İş cüzdanı kromu — fatura tahsilatı yok.',
            icon: Icons.apartment_outlined,
            selected: store.kind == kurumsalKind,
            onTap: () => onPicked(kurumsalKind),
          ),
          const DemoFooter(),
        ],
      ),
    );
  }
}

class _ModeCard extends StatelessWidget {
  const _ModeCard({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.selected,
    required this.onTap,
  });

  final String title;
  final String subtitle;
  final IconData icon;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: const BorderRadius.all(Radius.circular(12)),
        side: BorderSide(color: selected ? navy : line, width: selected ? 2 : 1),
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: const BorderRadius.all(Radius.circular(12)),
        child: Padding(
          padding: const EdgeInsets.all(18),
          child: Row(
            children: [
              Icon(icon, color: teal, size: 32),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(color: navy, fontSize: 18, fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: 4),
                    Text(subtitle, style: const TextStyle(color: muted, fontSize: 13)),
                  ],
                ),
              ),
              if (selected) const Icon(Icons.check_circle, color: navy),
            ],
          ),
        ),
      ),
    );
  }
}
