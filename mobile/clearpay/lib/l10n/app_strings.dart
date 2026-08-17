class L {
  const L(this.code);

  final String code;

  String _p(String tr, String en, String de, String fr) => switch (code) {
        'en' => en,
        'de' => de,
        'fr' => fr,
        _ => tr,
      };

  String money(num amount) {
    final fixed = amount.toStringAsFixed(2);
    final body = code == 'en' ? fixed : fixed.replaceAll('.', ',');
    return '$body ₺';
  }

  String get signIn => _p('Giriş', 'Sign in', 'Anmelden', 'Connexion');
  String get signInLede => _p(
        'Aynı SQL defteri. Bakiye telefonda tutulmaz.',
        'The same SQL ledger. Balance is not stored on the phone.',
        'Dasselbe SQL-Ledger. Saldo liegt nicht auf dem Telefon.',
        'Le même grand livre SQL. Le solde n’est pas sur le téléphone.',
      );
  String get email => _p('E-posta', 'Email', 'E-Mail', 'E-mail');
  String get password => _p('Şifre', 'Password', 'Passwort', 'Mot de passe');
  String get tcDemo => _p('TC (demo)', 'National ID (demo)', 'TC (Demo)', 'TC (démo)');
  String get tcHint => _p(
        'Mernis değil. Demo seed 10000000146 → admin@clearpay.test',
        'Not a government check. Demo seed 10000000146 → admin@clearpay.test',
        'Kein Mernis. Demo 10000000146 → admin@clearpay.test',
        'Pas Mernis. Démo 10000000146 → admin@clearpay.test',
      );
  String get forgot => _p('Şifremi unuttum', 'Forgot password', 'Passwort vergessen', 'Mot de passe oublié');
  String get createAccount => _p('Hesap oluştur', 'Create account', 'Konto erstellen', 'Créer un compte');
  String get demoFooter => _p(
        'Demo — sahte banka gateway.',
        'Demo — fake bank gateway.',
        'Demo — gefälschtes Bank-Gateway.',
        'Démo — passerelle bancaire factice.',
      );
  String firestoreWrote(String message) => _p(
        'Firestore yazıldı: app_meta/ping — $message (tutar yok, kasa SQL).',
        'Firestore wrote: app_meta/ping — $message (no amounts; ledger is SQL).',
        'Firestore geschrieben: app_meta/ping — $message (kein Betrag; Kasse SQL).',
        'Firestore écrit : app_meta/ping — $message (pas de montant ; caisse SQL).',
      );
  String get firestoreSkipped => _p(
        'Firestore ping atlandı (Windows native / init yok). Android emülatörde yazılır.',
        'Firestore ping skipped (no Windows native / init). Writes on the Android emulator.',
        'Firestore-Ping übersprungen (kein Windows-Native). Schreiben im Android-Emulator.',
        'Ping Firestore ignoré (pas de native Windows). Écriture sur l’émulateur Android.',
      );
  String firestoreFailed(String detail) => _p(
        'Firestore ping hata: $detail. JWT girişi durmaz.',
        'Firestore ping error: $detail. JWT sign-in still works.',
        'Firestore-Ping-Fehler: $detail. JWT-Anmeldung geht weiter.',
        'Erreur ping Firestore : $detail. La connexion JWT continue.',
      );
  String kindLogin(String kind) => _p(
        '$kind giriş',
        '$kind sign-in',
        '$kind Anmeldung',
        'Connexion $kind',
      );
  String get unknownTc => _p(
        'Bu demo TC tanımlı değil. Mernis yok. Seed: 10000000146 veya e-posta ile girin.',
        'This demo national ID is not mapped. Seed: 10000000146 or use email.',
        'Diese Demo-TC ist nicht hinterlegt. Seed: 10000000146 oder E-Mail.',
        'Ce TC démo n’est pas associé. Seed : 10000000146 ou e-mail.',
      );
  String get overview => _p('Özet', 'Overview', 'Übersicht', 'Aperçu');
  String get transfer => _p('Havale', 'Transfer', 'Überweisung', 'Virement');
  String get topUpWithdraw => _p('Yükle / Çek', 'Top up / Withdraw', 'Aufladen / Abheben', 'Recharger / Retirer');
  String get topUp => _p('Yükle', 'Top up', 'Aufladen', 'Recharger');
  String get withdraw => _p('Çek', 'Withdraw', 'Abheben', 'Retirer');
  String get movements => _p('Hareketler', 'Activity', 'Umsätze', 'Mouvements');
  String get movementShort => _p('Hareket', 'Activity', 'Umsatz', 'Mouvement');
  String get cards => _p('Kartlarım', 'Cards', 'Karten', 'Cartes');
  String get cardsLede => _p(
        'Numara Visa veya Mastercard yüzünü seçer. Tam numara kaydedilmez.',
        'The number picks a Visa or Mastercard face. The full number is not stored.',
        'Die Nummer wählt Visa- oder Mastercard-Gesicht. Die volle Nummer wird nicht gespeichert.',
        'Le numéro choisit Visa ou Mastercard. Le numéro complet n’est pas enregistré.',
      );
  String get cardNumber => _p('Kart numarası', 'Card number', 'Kartennummer', 'Numéro de carte');
  String get cardHolder => _p('Kart üzerindeki ad', 'Name on card', 'Name auf der Karte', 'Nom sur la carte');
  String get cardExpiry => _p('SKT', 'Expiry', 'Ablauf', 'Expiration');
  String get cardCvv => _p('CVV (kaydedilmez)', 'CVV (not stored)', 'CVV (nicht gespeichert)', 'CVV (non enregistré)');
  String get cardNickname => _p('Kart adı', 'Card nickname', 'Kartenname', 'Nom de la carte');
  String get noLinkedCard => _p('Kayıtlı kart yok.', 'No linked card.', 'Keine verknüpfte Karte.', 'Aucune carte liée.');
  String get loadFromCard => _p('Bu karttan cüzdana yükle', 'Load wallet from this card', 'Wallet von dieser Karte laden', 'Recharger depuis cette carte');
  String get receipt => _p('Dekont', 'Receipt', 'Beleg', 'Reçu');
  String get receiptFromList => _p('Hareketler listesinden', 'From the activity list', 'Aus der Umsatzliste', 'Depuis la liste');
  String get admin => _p('Admin', 'Admin', 'Admin', 'Admin');
  String get signOut => _p('Çıkış', 'Sign out', 'Abmelden', 'Déconnexion');
  String get notMerchant => _p('üye iş yeri değil', 'not a merchant', 'kein Händler', 'pas un commerçant');
  String get modeTitle => _p('Nasıl devam edilsin?', 'How to continue?', 'Wie fortfahren?', 'Comment continuer ?');
  String get modeLede => _p(
        'Aynı 8 işlem, aynı SQL defteri. Kurumsal üye iş yeri / POS değil.',
        'The same 8 operations, the same SQL ledger. Not a merchant POS.',
        'Dieselben 8 Vorgänge, dasselbe SQL-Ledger. Kein Händler-POS.',
        'Les 8 mêmes opérations, le même grand livre SQL. Pas de POS marchand.',
      );
  String get bireyselHint => _p(
        'Kişisel cüzdan görünümü.',
        'Personal wallet chrome.',
        'Persönliche Wallet-Ansicht.',
        'Vue portefeuille personnelle.',
      );
  String get kurumsalHint => _p(
        'İş cüzdanı kromu — fatura tahsilatı yok.',
        'Business wallet chrome — no invoicing.',
        'Geschäftliche Wallet — keine Rechnungsstellung.',
        'Portefeuille pro — pas de facturation.',
      );
  String get registerTitle => _p('Hesap oluştur', 'Create account', 'Konto erstellen', 'Créer un compte');
  String kindRegister(String kind) => _p(
        '$kind kayıt — Firebase Auth, kasa SQL',
        '$kind sign-up — Firebase Auth, SQL ledger',
        '$kind Registrierung — Firebase Auth, SQL-Ledger',
        'Inscription $kind — Firebase Auth, grand livre SQL',
      );
  String get fullName => _p('Ad', 'Name', 'Name', 'Nom');
  String get phone => _p('Telefon', 'Phone', 'Telefon', 'Téléphone');
  String get confirmPassword => _p('Şifre tekrar', 'Confirm password', 'Passwort wiederholen', 'Confirmer le mot de passe');
  String get haveAccount => _p('Zaten hesabın var? Giriş', 'Already have an account? Sign in', 'Konto vorhanden? Anmelden', 'Déjà un compte ? Connexion');
  String get passwordsMismatch => _p('Şifreler eşleşmiyor.', 'Passwords do not match.', 'Passwörter stimmen nicht überein.', 'Les mots de passe ne correspondent pas.');
  String get phoneRequired => _p('Telefon zorunludur.', 'Phone is required.', 'Telefon ist erforderlich.', 'Le téléphone est obligatoire.');
  String get forgotLede => _p(
        'E-posta veya kayıtlı telefon. TC/Mernis değil. Banka OTP değil.',
        'Email or registered phone. Not national ID. Not bank OTP.',
        'E-Mail oder registriertes Telefon. Kein TC/Mernis. Kein Bank-OTP.',
        'E-mail ou téléphone enregistré. Pas TC/Mernis. Pas d’OTP bancaire.',
      );
  String get lookupLabel => _p('E-posta veya telefon', 'Email or phone', 'E-Mail oder Telefon', 'E-mail ou téléphone');
  String get requestReset => _p('Sıfırlama iste', 'Request reset', 'Zurücksetzen anfordern', 'Demander la réinitialisation');
  String get resetCode => _p('Sıfırlama kodu (log)', 'Reset code (log)', 'Reset-Code (Log)', 'Code de réinitialisation (journal)');
  String get newPassword => _p('Yeni şifre', 'New password', 'Neues Passwort', 'Nouveau mot de passe');
  String get updatePassword => _p('Şifreyi güncelle', 'Update password', 'Passwort aktualisieren', 'Mettre à jour le mot de passe');
  String get lookupEmpty => _p('E-posta veya telefon girin.', 'Enter email or phone.', 'E-Mail oder Telefon eingeben.', 'Saisissez e-mail ou téléphone.');
  String get resetNeedsEmail => _p(
        'Kod ile sıfırlamak için hesabın e-postasını girin.',
        'Enter the account email to reset with a code.',
        'Geben Sie die Konto-E-Mail ein, um mit Code zurückzusetzen.',
        'Saisissez l’e-mail du compte pour réinitialiser avec un code.',
      );
  String get firebaseEmailSent => _p(
        'Firebase e-posta gönderildi. Gelen kutusu (veya spam). SMS yok. Sonra girişe dönün.',
        'Firebase email sent. Check inbox (or spam). No SMS. Then return to sign-in.',
        'Firebase-E-Mail gesendet. Posteingang (oder Spam). Kein SMS. Dann zur Anmeldung.',
        'E-mail Firebase envoyé. Boîte de réception (ou spam). Pas de SMS. Puis connexion.',
      );
  String get identityResetInfo => _p(
        'Varsa e-posta kuyruğa alındı. Demo: Development site log’undaki sıfırlama kodunu yapıştırın. Production ekranda token yok. SMS fatura yok.',
        'If the account exists, email was queued. Demo: paste the reset code from the Development site log. No token on the Production screen. No SMS.',
        'Falls vorhanden, E-Mail in der Queue. Demo: Reset-Code aus dem Development-Log einfügen. Production zeigt kein Token. Kein SMS.',
        'Si le compte existe, e-mail en file. Démo : collez le code du journal Development. Pas de jeton à l’écran Production. Pas de SMS.',
      );

  String get wallet => _p('Cüzdan', 'Wallet', 'Wallet', 'Portefeuille');
  String get corporateWallet => _p(
        'Kurumsal cüzdan (demo — üye iş yeri değil)',
        'Business wallet (demo — not a merchant)',
        'Geschäftliche Wallet (Demo — kein Händler)',
        'Portefeuille pro (démo — pas un commerçant)',
      );
  String get quickOps => _p('Hızlı işlemler', 'Quick actions', 'Schnellzugriff', 'Accès rapide');
  String get corporateShortcuts => _p(
        'Kurumsal kısayollar (demo)',
        'Business shortcuts (demo)',
        'Geschäftliche Shortcuts (Demo)',
        'Raccourcis pro (démo)',
      );
  String get qrReceive => _p('QR al', 'Receive QR', 'QR empfangen', 'QR recevoir');
  String get qrPay => _p('QR öde', 'Pay QR', 'QR zahlen', 'QR payer');
  String get fast => _p('FAST', 'FAST', 'FAST', 'FAST');
  String get markets => _p('Piyasalar', 'Markets', 'Märkte', 'Marchés');
  String get more => _p('Daha fazla', 'More', 'Mehr', 'Plus');
  String get fastHint => _p(
        'FAST kiremiti Havale’ye gider. QR kanalı; Jet QR / World Pay değil.',
        'The FAST tile opens Transfer. QR channel; not Jet QR / World Pay.',
        'FAST-Kachel öffnet Überweisung. QR-Kanal; nicht Jet QR / World Pay.',
        'La tuile FAST ouvre Virement. Canal QR ; pas Jet QR / World Pay.',
      );
  String get recentActivity => _p('Son hareketler', 'Recent activity', 'Letzte Bewegungen', 'Derniers mouvements');
  String get viewAll => _p('Tümü', 'View all', 'Alle', 'Tout');
  String get noMovementsYet => _p('Henüz hareket yok.', 'No activity yet.', 'Noch keine Bewegungen.', 'Aucun mouvement pour l’instant.');
  String monthFlow(String outgoing, String incoming) => _p(
        'Bu ay giden $outgoing  ·  gelen $incoming',
        'Out this month $outgoing  ·  in $incoming',
        'Diesen Monat ausgehend $outgoing  ·  eingehend $incoming',
        'Ce mois sortant $outgoing  ·  entrant $incoming',
      );
  String get walletFrozen => _p('Cüzdan dondurulmuş', 'Wallet frozen', 'Wallet gesperrt', 'Portefeuille gelé');
  String get parkNotDemo => _p('Park — demo değil', 'Park — not in the demo', 'Park — nicht in der Demo', 'Park — hors démo');
  String get marketsPark => _p('Yatırım/döviz yok; SPEC 8.', 'No investments/FX; SPEC 8.', 'Kein Investment/FX; SPEC 8.', 'Pas d’investissement/change ; SPEC 8.');
  String get invoice => _p('Fatura', 'Invoice', 'Rechnung', 'Facture');
  String get credit => _p('Kredi', 'Credit', 'Kredit', 'Crédit');
  String get movementsReceipt => _p('Hareketler / Dekont', 'Activity / Receipt', 'Umsätze / Beleg', 'Mouvements / Reçu');
  String get qrNeedsEmail => _p('QR için JWT e-posta yok.', 'No JWT email for QR.', 'Keine JWT-E-Mail für QR.', 'Pas d’e-mail JWT pour le QR.');
  String get receiveQrTitle => _p('QR ile al', 'Receive with QR', 'Mit QR empfangen', 'Recevoir par QR');
  String get receiveQrLede => _p(
        'ClearPay demo URI. TCMB FAST / Jet QR değil.',
        'ClearPay demo URI. Not TCMB FAST / Jet QR.',
        'ClearPay-Demo-URI. Nicht TCMB FAST / Jet QR.',
        'URI démo ClearPay. Pas TCMB FAST / Jet QR.',
      );
  String get copyUri => _p('URI kopyala', 'Copy URI', 'URI kopieren', 'Copier l’URI');
  String get payQrTitle => _p('QR ile öde', 'Pay with QR', 'Mit QR zahlen', 'Payer par QR');
  String get payQrLede => _p(
        'Kamera yoksa URI yapıştır veya alıcı e-posta yaz. Havale formuna dolar; POST /api/transfers.',
        'No camera: paste a URI or recipient email. Fills the transfer form; POST /api/transfers.',
        'Ohne Kamera: URI oder Empfänger-E-Mail. Füllt das Überweisungsformular; POST /api/transfers.',
        'Sans caméra : collez un URI ou un e-mail. Remplit le virement ; POST /api/transfers.',
      );
  String get qrPayload => _p('QR yükü veya alıcı e-posta', 'QR payload or recipient email', 'QR-Last oder Empfänger-E-Mail', 'Charge QR ou e-mail destinataire');
  String get invalidQr => _p(
        'Geçerli ClearPay QR veya e-posta girin.',
        'Enter a valid ClearPay QR or email.',
        'Gültigen ClearPay-QR oder E-Mail eingeben.',
        'Saisissez un QR ClearPay ou un e-mail valide.',
      );
  String get fillTransferForm => _p('Havale formunu doldur', 'Fill transfer form', 'Überweisungsformular füllen', 'Remplir le virement');
  String get fastSnack => _p('Demo P2P — TCMB FAST değil', 'Demo P2P — not TCMB FAST', 'Demo-P2P — nicht TCMB FAST', 'P2P démo — pas TCMB FAST');

  String get frozenNoSend => _p(
        'Cüzdan dondurulmuş; gönderim kapalı.',
        'Wallet frozen; sending is closed.',
        'Wallet gesperrt; Senden geschlossen.',
        'Portefeuille gelé ; envoi fermé.',
      );
  String get pasteQr => _p('QR yapıştır', 'Paste QR', 'QR einfügen', 'Coller le QR');
  String get qrOrEmail => _p('clearpay://pay?to=… veya e-posta', 'clearpay://pay?to=… or email', 'clearpay://pay?to=… oder E-Mail', 'clearpay://pay?to=… ou e-mail');
  String get cancel => _p('İptal', 'Cancel', 'Abbrechen', 'Annuler');
  String get fill => _p('Doldur', 'Fill', 'Füllen', 'Remplir');
  String get qrFilledForm => _p(
        'QR alıcı forma yazıldı. Onay + POST /api/transfers.',
        'QR recipient written to the form. Confirm + POST /api/transfers.',
        'QR-Empfänger ins Formular. Bestätigen + POST /api/transfers.',
        'Destinataire QR dans le formulaire. Confirmer + POST /api/transfers.',
      );
  String get validAmount => _p('Geçerli tutar girin.', 'Enter a valid amount.', 'Gültigen Betrag eingeben.', 'Saisissez un montant valide.');
  String get confirmTransfer => _p('Havale onayı', 'Confirm transfer', 'Überweisung bestätigen', 'Confirmer le virement');
  String confirmSend(String recipient, String amount) => _p(
        '$recipient hesabına $amount gönderilsin mi?',
        'Send $amount to $recipient?',
        '$amount an $recipient senden?',
        'Envoyer $amount à $recipient ?',
      );
  String get send => _p('Gönder', 'Send', 'Senden', 'Envoyer');
  String remainingBalance(String amount) => _p(
        'Kalan bakiye: $amount',
        'Remaining: $amount',
        'Restsaldo: $amount',
        'Solde restant : $amount',
      );
  String get payQrIsThisForm => _p(
        'QR ile öde bu formdur. Jet QR değil.',
        'Pay-with-QR is this form. Not Jet QR.',
        'QR-Zahlen ist dieses Formular. Nicht Jet QR.',
        'Payer par QR = ce formulaire. Pas Jet QR.',
      );
  String get recipientEmail => _p('Alıcı e-posta', 'Recipient email', 'Empfänger-E-Mail', 'E-mail destinataire');
  String get amount => _p('Tutar', 'Amount', 'Betrag', 'Montant');
  String get description => _p('Açıklama', 'Description', 'Beschreibung', 'Description');
  String get frozen => _p('Dondurulmuş', 'Frozen', 'Gesperrt', 'Gelé');

  String get frozenNoFunding => _p(
        'Cüzdan dondurulmuş; yükle/çek kapalı.',
        'Wallet frozen; top-up/withdraw closed.',
        'Wallet gesperrt; Aufladen/Abheben geschlossen.',
        'Portefeuille gelé ; recharger/retirer fermé.',
      );
  String get cardAdded => _p(
        'Kart eklendi (PAN yok, yalnız son 4).',
        'Card added (no PAN, last 4 only).',
        'Karte hinzugefügt (kein PAN, nur letzte 4).',
        'Carte ajoutée (pas de PAN, 4 derniers seulement).',
      );
  String get linkedCard => _p('Kayıtlı kart (demo, PAN yok)', 'Linked card (demo, no PAN)', 'Verknüpfte Karte (Demo, kein PAN)', 'Carte liée (démo, pas de PAN)');
  String get last4 => _p('Son 4 hane', 'Last 4 digits', 'Letzte 4 Ziffern', '4 derniers chiffres');
  String get cardLabel => _p('Etiket', 'Label', 'Bezeichnung', 'Libellé');
  String get addCard => _p('Kart ekle', 'Add card', 'Karte hinzufügen', 'Ajouter une carte');
  String get accountHint => _p(
        'Hesap ipucu (TIMEOUT = zaman aşımı)',
        'Account hint (TIMEOUT = timeout)',
        'Kontohinweis (TIMEOUT = Zeitüberschreitung)',
        'Indice de compte (TIMEOUT = délai dépassé)',
      );

  String get filterAll => _p('Tümü', 'All', 'Alle', 'Tous');
  String get topUpKind => _p('Yükleme', 'Top-up', 'Aufladung', 'Recharge');
  String get withdrawKind => _p('Çekim', 'Withdrawal', 'Abhebung', 'Retrait');
  String get filter => _p('Filtrele', 'Filter', 'Filtern', 'Filtrer');
  String get filterFrom => _p('Başlangıç', 'From', 'Von', 'Début');
  String get filterTo => _p('Bitiş', 'To', 'Bis', 'Fin');
  String get clear => _p('Temizle', 'Clear', 'Leeren', 'Effacer');
  String get emptyPeriod => _p('Bu dönemde hareket yok.', 'No activity in this period.', 'Keine Bewegungen in diesem Zeitraum.', 'Aucun mouvement sur cette période.');
  String get previous => _p('Önceki', 'Previous', 'Zurück', 'Précédent');
  String get next => _p('Sonraki', 'Next', 'Weiter', 'Suivant');

  String get copied => _p('Kopyalandı', 'Copied', 'Kopiert', 'Copié');
  String get kindLabel => _p('Tür', 'Type', 'Art', 'Type');
  String get debitParty => _p('Borç', 'Debit', 'Soll', 'Débit');
  String get creditParty => _p('Alacak', 'Credit', 'Haben', 'Crédit');
  String get time => _p('Zaman', 'Time', 'Zeit', 'Heure');
  String get correlation => _p('Correlation', 'Correlation', 'Correlation', 'Correlation');
  String get accountCard => _p('Hesap / kart', 'Account / card', 'Konto / Karte', 'Compte / carte');
  String get copy => _p('Kopyala', 'Copy', 'Kopieren', 'Copier');
  String get downloadPdf => _p('PDF indir', 'Download PDF', 'PDF herunterladen', 'Télécharger le PDF');

  String get freezeWallet => _p('Cüzdan dondur', 'Freeze wallet', 'Wallet sperren', 'Geler le portefeuille');
  String get freeze => _p('Dondur', 'Freeze', 'Sperren', 'Geler');
  String get unfreeze => _p('Çöz', 'Unfreeze', 'Entsperren', 'Dégeler');
  String get frozenMsg => _p('Donduruldu.', 'Frozen.', 'Gesperrt.', 'Gelé.');
  String get unfrozenMsg => _p('Çözüldü.', 'Unfrozen.', 'Entsperrt.', 'Dégelé.');
  String get failedQueue => _p('Başarısız kuyruk', 'Failed queue', 'Fehler-Queue', 'File d’échec');
  String get requeue => _p('Kuyruğa al', 'Requeue', 'Erneut einreihen', 'Remettre en file');
  String get audit => _p('Audit', 'Audit', 'Audit', 'Audit');
  String get searchActor => _p('Aktör ara', 'Search actor', 'Akteur suchen', 'Rechercher l’acteur');
  String get correlationId => _p('Correlation id', 'Correlation id', 'Correlation-ID', 'Id de corrélation');
  String get search => _p('Ara', 'Search', 'Suchen', 'Rechercher');
}
