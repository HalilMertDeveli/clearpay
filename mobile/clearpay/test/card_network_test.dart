import 'package:clearpay/cards/card_network.dart';
import 'package:clearpay/cards/live_payment_card.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('ISO IIN prefixes match Visa Mastercard and Troy', () {
    expect(CardNetwork.detect('4111111111111111'), CardScheme.visa);
    expect(CardNetwork.detect('5555555555554444'), CardScheme.mastercard);
    expect(CardNetwork.detect('2223003122003222'), CardScheme.mastercard);
    expect(CardNetwork.detect('9792123412341234'), CardScheme.troy);
    expect(CardNetwork.detect('34'), CardScheme.unknown);
    expect(CardNetwork.parseStored('Mastercard'), CardScheme.mastercard);
  });

  testWidgets('Visa number paints VISA mark on a blue face', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: LivePaymentCard(digits: '4111111111111111', holder: 'AYSE'),
        ),
      ),
    );
    expect(find.text('VISA'), findsOneWidget);
    expect(find.textContaining('4111'), findsOneWidget);
  });

  testWidgets('Mastercard number paints overlapping orbs not VISA', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: LivePaymentCard(digits: '5555555555554444', holder: 'ALI'),
        ),
      ),
    );
    expect(find.text('VISA'), findsNothing);
    expect(find.byType(CustomPaint), findsWidgets);
  });
}
