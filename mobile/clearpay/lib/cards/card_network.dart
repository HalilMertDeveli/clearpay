/// ISO/IEC 7812 IIN (BIN) prefixes. Same rules as C# CardNetwork.
enum CardScheme { visa, mastercard, troy, unknown }

class CardNetwork {
  static const visa = 'Visa';
  static const mastercard = 'Mastercard';
  static const troy = 'Troy';
  static const unknown = 'Unknown';

  static CardScheme detect(String digits) {
    if (digits.isEmpty) {
      return CardScheme.unknown;
    }
    if (digits.startsWith('9792')) {
      return CardScheme.troy;
    }
    if (digits.startsWith('4')) {
      return CardScheme.visa;
    }
    if (_isMastercard(digits)) {
      return CardScheme.mastercard;
    }
    return CardScheme.unknown;
  }

  static CardScheme parseStored(String? scheme) {
    switch ((scheme ?? '').toLowerCase()) {
      case 'visa':
        return CardScheme.visa;
      case 'mastercard':
        return CardScheme.mastercard;
      case 'troy':
        return CardScheme.troy;
      default:
        return CardScheme.unknown;
    }
  }

  static String label(CardScheme scheme) => switch (scheme) {
        CardScheme.visa => visa,
        CardScheme.mastercard => mastercard,
        CardScheme.troy => troy,
        CardScheme.unknown => unknown,
      };

  static bool _isMastercard(String digits) {
    if (digits.length >= 2) {
      final two = int.tryParse(digits.substring(0, 2));
      if (two != null && two >= 51 && two <= 55) {
        return true;
      }
    }
    if (digits.length >= 4) {
      final four = int.tryParse(digits.substring(0, 4));
      if (four != null && four >= 2221 && four <= 2720) {
        return true;
      }
    }
    return false;
  }
}
