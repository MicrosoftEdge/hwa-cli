interface HWAAdapter {
    launchAppx(): void;
    registerAndLaunchAppxManifest(path: string): void;
}

interface HWAProxyAdapter extends HWAAdapter {
    clearSession(): void;
    exit(): void;
}