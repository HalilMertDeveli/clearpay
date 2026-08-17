import 'package:flutter/material.dart';

import '../cards/card_network.dart';
import '../theme.dart';

const visaBlue = Color(0xFF1A237E);
const visaBlueMid = Color(0xFF0D47A1);
const mcDark = Color(0xFF1A1A1A);
const mcRed = Color(0xFFEB001B);
const mcGold = Color(0xFFF79E1B);
const troyDeep = Color(0xFF134E4A);

class LivePaymentCard extends StatelessWidget {
  const LivePaymentCard({
    super.key,
    required this.digits,
    this.holder = '',
    this.expiry = '',
    this.scheme,
    this.last4,
  });

  final String digits;
  final String holder;
  final String expiry;
  final CardScheme? scheme;
  final String? last4;

  @override
  Widget build(BuildContext context) {
    final clean = digits.replaceAll(RegExp(r'\D'), '');
    final resolved = scheme ?? CardNetwork.detect(clean);
    final pan = _panText(clean, last4);
    return Container(
      width: double.infinity,
      height: 196,
      padding: const EdgeInsets.fromLTRB(18, 16, 18, 14),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(18),
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: _face(resolved),
        ),
      ),
      child: Stack(
        children: [
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'ClearPay',
                style: TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.8,
                  fontSize: 13,
                ),
              ),
              const Spacer(),
              Text(
                pan,
                style: const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.w600,
                  letterSpacing: 2.2,
                  fontSize: 18,
                ),
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      holder.isEmpty ? ' ' : holder.toUpperCase(),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w600,
                        fontSize: 13,
                      ),
                    ),
                  ),
                  Text(
                    expiry.isEmpty ? 'MM/YY' : expiry,
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ],
          ),
          Positioned(
            top: 0,
            right: 0,
            child: _SchemeMark(scheme: resolved),
          ),
        ],
      ),
    );
  }

  static List<Color> _face(CardScheme scheme) => switch (scheme) {
        CardScheme.visa => const [visaBlue, visaBlueMid, Color(0xFF1565C0)],
        CardScheme.mastercard => const [Color(0xFF3A3A3A), mcDark],
        CardScheme.troy => const [teal, troyDeep],
        CardScheme.unknown => const [navyHero, navy],
      };

  static String _panText(String digits, String? last4) {
    if (digits.isEmpty && last4 != null && last4.isNotEmpty) {
      return '•••• •••• •••• $last4';
    }
    final padded = '$digits••••••••••••••••'.substring(0, 16);
    final groups = <String>[];
    for (var i = 0; i < 16; i += 4) {
      groups.add(padded.substring(i, i + 4));
    }
    return groups.join(' ');
  }
}

class _SchemeMark extends StatelessWidget {
  const _SchemeMark({required this.scheme});

  final CardScheme scheme;

  @override
  Widget build(BuildContext context) {
    if (scheme == CardScheme.mastercard) {
      return const SizedBox(
        width: 56,
        height: 34,
        child: CustomPaint(painter: _MastercardOrbsPainter()),
      );
    }
    final text = switch (scheme) {
      CardScheme.visa => 'VISA',
      CardScheme.troy => 'TROY',
      CardScheme.unknown => '',
      CardScheme.mastercard => '',
    };
    return Text(
      text,
      style: TextStyle(
        color: Colors.white,
        fontWeight: FontWeight.w800,
        fontStyle: scheme == CardScheme.visa ? FontStyle.italic : FontStyle.normal,
        letterSpacing: scheme == CardScheme.visa ? 2.4 : 1.2,
        fontSize: 16,
      ),
    );
  }
}

class _MastercardOrbsPainter extends CustomPainter {
  const _MastercardOrbsPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final r = size.height * 0.42;
    final y = size.height / 2;
    canvas.drawCircle(Offset(r, y), r, Paint()..color = mcRed);
    canvas.drawCircle(Offset(size.width - r, y), r, Paint()..color = mcGold.withValues(alpha: 0.92));
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
