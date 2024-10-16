@echo off
setlocal enabledelayedexpansion

for /d %%d in (*) do (
    set "file_exit="
    for %%f in (%%d\*) do (
        set "file_exit=1"
    )
    if not defined file_exit (
        echo Creating .gitkeep in %%d
        type nul > "%%d\.gitkeep"
    )
)

endlocal
