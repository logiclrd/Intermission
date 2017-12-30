bin\Debug\RawVideo.exe | c:\ffmpeg\bin\ffmpeg.exe -f rawvideo -pixel_format rgb32 -video_size 1280x720 -framerate 30000/1001 -i - -c:v libx264 -b:v 2000k -c:a null -y IntermissionVideo.mkv
