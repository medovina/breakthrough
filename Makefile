# Build Breakthrough on Linux

SOURCES = clever.cs game.cs my_agent.cs util.cs

breakthrough_gtk.exe: $(SOURCES) view_gtk.cs
	csc $^ `pkg-config --libs gtk-sharp-2.0` -r:Mono.Cairo.dll -out:$@
	
breakthrough_winforms.exe: $(SOURCES) view_winforms.cs
	csc $^ -out:$@

clean:
	rm *.exe
