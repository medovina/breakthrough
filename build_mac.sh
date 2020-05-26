csc -lib:/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/gtk-sharp-2.0 \
    -r:gdk-sharp.dll,glib-sharp.dll,gtk-sharp.dll,Mono.Cairo.dll \
    clever.cs game.cs my_agent.cs util.cs view_gtk.cs -out:breakthrough_gtk.exe