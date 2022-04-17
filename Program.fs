open Microsoft.Playwright
open System.Threading.Tasks
open FSharp.Control.Tasks
open System.IO
open System.Text.Json

type Browser =
    | Chromium
    | Chrome
    | Edge
    | Firefox
    | Webkit

    member instance.AsString =
        match instance with
        | Chromium -> "Chromium"
        | Chrome -> "Google Chrome"
        | Edge -> "Edge"
        | Firefox -> "Firefox"
        | Webkit -> "Webkit"


let getBrowser (kind: Browser) (getPlaywright: Task<IPlaywright>) =
    task {
        let! pl = getPlaywright
        printfn $"Browsing with {kind.AsString}"

        return!
            match kind with
            | Chromium -> pl.Chromium.LaunchAsync()
            | Chrome ->
                let opts = BrowserTypeLaunchOptions()
                opts.Channel <- "chrome"
                pl.Chromium.LaunchAsync(opts)
            | Edge ->
                let opts= BrowserTypeLaunchOptions()
                opts.Channel <- "msedge"
                pl.Chromium.LaunchAsync(opts)
            | Firefox -> pl.Firefox.LaunchAsync()
            | Webkit -> pl.Webkit.LaunchAsync()
    }

let getPage (url:string) (getBrowser: Task<IBrowser>) =
    task{
        let! browser = getBrowser
        printfn $"Navigating to \"{url}\""

        let! page = browser.NewPageAsync()
        let! res = page.GotoAsync url
        if not res.Ok then
            return failwith "Couldn't navigate to page."
        
        return page
    }

let printElement e =
    printfn e

let getProduct (getPage:Task<IPage>) =
    task {
        let! page = getPage
        let! addCartButton = page.QuerySelectorAsync("#add-to-cart")
        // printfn $"Getting 'Add To Cart' button: %A" addCartButton
        return! 
            addCartButton.AsElement().IsDisabledAsync()
    }

let xbox = "https://www.gamestop.com/pc-gaming/pc-gaming-controllers/products/microsoft-xbox-series-x-wireless-controller-carbon-black/11108954.html"
let arcade = "https://www.gamestop.com/consoles-hardware/arcade/table-top-arcades/products/atgames-legends-gamer-pro-console/227025.html"

[<EntryPoint>]
let main _ =
    Playwright.CreateAsync()
    |> getBrowser Webkit
    |> getPage arcade
    |> getProduct
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ( function
        | false -> printfn "Is Available 💥"
        | true -> printfn "Nuthin 😢"
    )
    0