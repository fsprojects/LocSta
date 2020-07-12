function success() {
    if ($LASTEXITCODE -ne 0) {
        write-host "----------------"
        write-host "---------- ERROR:" $LASTEXITCODE 
        write-host "----------------"
        exit
    }
}
