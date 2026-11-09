/// Demo TCKN → Identity e-posta. Mernis / KYC yok.
const demoAdminTc = '10000000146';
const demoAdminEmail = 'admin@clearpay.test';

const Map<String, String> demoTcToEmail = {
  demoAdminTc: demoAdminEmail,
};

String digitsOnly(String raw) => raw.replaceAll(RegExp(r'\D'), '');

/// Returns mapped e-posta, or null if this demo TC is unknown.
String? resolveDemoTcEmail(String raw) => demoTcToEmail[digitsOnly(raw)];
