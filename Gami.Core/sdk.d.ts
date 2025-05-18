export enum AddonConfigSettingType {
    String = "string",
    Int = "int",
    Bool = "bool",
}
export interface AddonSetting {
    key: string
    name: string
    hint?: string
    type?: AddonConfigSettingType

}
export interface BaseConfig {
    key: string
    name: string
    settings: AddonSetting[]
}
export interface GameRef {
    name: string
    libraryId: string
    libraryType: string
}

export enum GameInstallStatus {
    Installed,
    Installing,
    InLibrary,
    Queued,
    Uninstalling,
}
export interface LibraryAddon extends BaseConfig {
    needsAuth?: boolean
    onCurrUrlChange?: (url: string) => Promise<boolean>
    authUrl?: string
    launch(game: GameRef): void
    install(game: GameRef): Promise<void>
    uninstall(game: GameRef): Promise<void>
    checkInstallStatus(game: GameRef): Promise<GameInstallStatus>
}
declare module "fs" {

    export interface FileFormats {
        text: string
        bytes: Uint8Array
    }
    function readFile<TFormat extends keyof FileFormats>(path: string, format: TFormat): Promise<FileFormats[TFormat]>
}
declare function getOsKind(): "mac" | "linux" | "windows" | null

declare module "path" {
    function join(a: string, b: string): string

    function exists(path: string): boolean

    function getRelative(relativeTo: string, path: string): string

    function getFull(path: string): string

    function getDirectoryName(path: string): string

}

/** Run a command or URL */
declare function runCommandOrUrl(command: string): Promise<boolean>

declare function registerLibrary(library: LibraryAddon): void;