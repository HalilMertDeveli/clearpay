import 'package:flutter/material.dart';

import '../theme.dart';
import 'locale_scope.dart';
import 'locale_store.dart';

class LanguageStrip extends StatelessWidget {
  const LanguageStrip({super.key, this.light = false});

  final bool light;

  @override
  Widget build(BuildContext context) {
    final scope = LocaleScope.maybeOf(context);
    if (scope == null) {
      return const SizedBox.shrink();
    }
    final active = light ? Colors.white : navy;
    final idle = light ? Colors.white70 : muted;
    return Wrap(
      spacing: 4,
      children: [
        for (final code in supportedLocales)
          TextButton(
            onPressed: () async {
              await scope.store.save(code);
              scope.onChanged();
            },
            style: TextButton.styleFrom(
              minimumSize: Size.zero,
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
            ),
            child: Text(
              switch (code) {
                'en' => 'English',
                'de' => 'Deutsch',
                'fr' => 'Français',
                _ => 'Türkçe',
              },
              style: TextStyle(
                fontSize: 12,
                fontWeight: scope.code == code ? FontWeight.w700 : FontWeight.w500,
                color: scope.code == code ? active : idle,
              ),
            ),
          ),
      ],
    );
  }
}
