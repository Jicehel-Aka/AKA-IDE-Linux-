PREFIX ?= /usr/local

all:
	@echo "Construire le projet avec: dotnet build GamebuinoAKA.sln -c Release"

install:
	install -d $(DESTDIR)$(PREFIX)/bin
	install -d $(DESTDIR)$(PREFIX)/share/applications
	install -d $(DESTDIR)/etc/udev/rules.d
	install -m 644 packaging/linux/gamebuino-aka.desktop $(DESTDIR)$(PREFIX)/share/applications/
	install -m 644 packaging/linux/99-gamebuino.rules $(DESTDIR)/etc/udev/rules.d/
	@echo "Règles et lanceurs installés avec succès."
