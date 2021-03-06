namespace Tests

open NUnit.Framework
open FSharper.Core
open FsUnit
            
[<TestClass>]
type TestClass () =

    let formatFsharp (s:string) = 
        s
            .Replace("                ", "")
            .Replace("\n    \n", "\n\n")
            .Replace("\n            \n", "\n\n")


    [<Test>]
    member this.``using statements convert to open statements`` () = 
        """using MvvmCross.Forms.Views;
           using TipCalc.Core.ViewModels; """ 
        |> Converter.run |> should equal (
            formatFsharp 
               "open MvvmCross.Forms.Views
                open TipCalc.Core.ViewModels")
        

    [<Test>]
    member this.``Can convert property and auto property`` () = 
        let csharp = 
             """using MvvmCross.Forms.Views;
                using TipCalc.Core.ViewModels;

                namespace TipCalc.Forms.UI.Pages
                {
                    public class TipView
                    {
                        private int _foo = 34;
                        public int Foo { 
                            get { return _foo; } 
                            set {
                                _foo = value;
                                RaisePropertyChanged("Foo");
                            } }

                        public Baz Bar { get; set; }
                        public Baz FooBar { get { return _foo; }}                        
                    }
                } """
            
        let fsharp = 
            """namespace TipCalc.Forms.UI.Pages

                open MvvmCross.Forms.Views
                open TipCalc.Core.ViewModels

                type TipView() =
                    let mutable _foo = 34

                    member this.Foo
                        with get () = _foo
                        and set value =
                            _foo <- value
                            RaisePropertyChanged("Foo")

                    member val Bar: Baz = null with get, set
                    member this.FooBar = _foo"""

        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)


    [<Test>]
    member this.``Can convert class method`` () = 
        let csharp = 
            """public void Foo(Class a, HudidIt fifity) {
                    Console.WriteLine("Hello, world");
                } """

        let fsharp = 
            """member this.Foo(a: Class, fifity: HudidIt) = Console.WriteLine("Hello, world")"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)


    [<Test>]
    member this.``Can Android Application`` () = 
        let csharp = 
            """[Activity(Label = "Activity A", MainLauncher = true)]
                public class MainApplication : MvxAndroidApplication
                {
                    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
                    {
                    }
                }"""

        let fsharp = 
            """[<Activity(Label = "Activity A", MainLauncher = True)>]
                type MainApplication(javaReference: IntPtr, transfer: JniHandleOwnership) =
                    inherit MvxAndroidApplication(javaReference, transfer)"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``can convert if statements`` () = 
        let csharp = 
            """[CommandHandler (RefactoryCommands.QuickFix)]
                void OnQuickFixCommand ()
                {
                    if (!AnalysisOptions.EnableFancyFeatures || smartTagMarginMarker == null) 
                    {
                        //Fixes = RefactoringService.GetValidActions (Editor, DocumentContext, Editor.CaretLocation).Result;
                        PopupQuickFixMenu (null, null);
                        return;
                    }

                    CancelSmartTagPopupTimeout ();
                    PopupQuickFixMenu (null, menu => { });
                
                }"""

        let fsharp = 
            """[<CommandHandler(RefactoryCommands.QuickFix)>]
                member this.OnQuickFixCommand() =
                    if not AnalysisOptions.EnableFancyFeatures || smartTagMarginMarker = null then
                        PopupQuickFixMenu(null, null)
                        ()
                    else
                        CancelSmartTagPopupTimeout()
                        PopupQuickFixMenu(null, fun menu -> ())"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``mulitple attributes`` () = 
        let csharp = 
             """using System;
                using Android.App;
                using Firebase.Iid;
                using Android.Util;

                namespace FCMClient
                {
                    [Service]
                    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
                    public class MyFirebaseIIDService : FirebaseInstanceIdService
                    {
                        const string TAG = "MyFirebaseIIDService";
                        public override void OnTokenRefresh()
                        {
                            var refreshedToken = FirebaseInstanceId.Instance.Token;
                            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
                            SendRegistrationToServer(refreshedToken);
                        }
                        void SendRegistrationToServer(string token)
                        {
                            // Add custom implementation, as needed.
                        }
                    }
                }"""

        let fsharp = 
            """namespace FCMClient

                open Android.Util
                open Firebase.Iid
                open System
                open Android.App
            
                [<Service(); IntentFilter([| "com.google.firebase.INSTANCE_ID_EVENT" |])>]
                type MyFirebaseIIDService() =
                    inherit FirebaseInstanceIdService()
                    let TAG = "MyFirebaseIIDService"

                    member this.OnTokenRefresh() =
                        let mutable refreshedToken = FirebaseInstanceId.Instance.Token
                        Log.Debug(TAG, "Refreshed token: " + refreshedToken)
                        this.SendRegistrationToServer(refreshedToken)
                
                    member this.SendRegistrationToServer(token: string) = ()"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

        

    [<Test>]
    member this.``single line`` () = 
        let csharp = 
            """AppCenter.Start("cf32a57f-60ce-4a42-aac1-a84148c29b0d", typeof(Push));"""

        let fsharp = 
            """AppCenter.Start("cf32a57f-60ce-4a42-aac1-a84148c29b0d", typeof<Push>)"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)
        
    [<Test>]
    member this.``create class instance`` () = 
        let csharp = 
             """void SendNotification(string messageBody)
                {
                    var intent = new Intent(this, typeof(MainActivity));
                    intent.AddFlags(ActivityFlags.ClearTop);
                    var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

                    var notificationBuilder = new Notification.Builder(this)
                                .SetContentTitle("FCM Message")
                                .SetSmallIcon(Resource.Drawable.ic_launcher)
                                .SetContentText(messageBody)
                                .SetAutoCancel(true)
                                .SetContentIntent(pendingIntent);

                    var notificationManager = NotificationManager.FromContext(this);
                    notificationManager.Notify(0, notificationBuilder.Build());
                }"""

        let fsharp = 
             """member this.SendNotification(messageBody: string) =
                    let mutable intent = new Intent(this, typeof<MainActivity>)
                    intent.AddFlags(ActivityFlags.ClearTop)
                    let mutable pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot)
                    let mutable notificationBuilder =
                        new Notification.Builder(this).SetContentTitle("FCM Message").SetSmallIcon(Resource.Drawable.ic_launcher)
                            .SetContentText(messageBody).SetAutoCancel(True).SetContentIntent(pendingIntent)
                    let mutable notificationManager = NotificationManager.FromContext(this)
                    notificationManager.Notify(0, notificationBuilder.Build())"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)


    [<Test>]
    member this.``convert const`` () = 
        let csharp = 
             """class CodeActionEditorExtension : TextEditorExtension
                {
                    const int menuTimeout = 150;
                }"""

        let fsharp = 
            """type CodeActionEditorExtension() =
                    inherit TextEditorExtension()
                    let menuTimeout = 150"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert on clicked handler`` () = 
        let csharp = 
             """ContextMenu CreateContextMenu (CodeFixMenu entrySet)
                {
                    menuItem.Clicked += (sender, e) => sender.Foo();
                }"""

        let fsharp = 
             """member this.CreateContextMenu(entrySet: CodeFixMenu) =
                    menuItem.Clicked.AddHandler<_> (fun (sender, e) -> sender.Foo())"""
                   
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert if else statement with an early return in else`` () = 
        let csharp = 
             """public int Foo()
                {
                    if (x)
                    {
                        SomeAction();
                    } 
                    else {
                        return f;
                    }
    
                    return 0;
                }"""

        let fsharp = 
             """member this.Foo() =
                    if x then
                        SomeAction()
                        0
                    else f"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert if else statement with early return if first if`` () = 
        let csharp = 
             """public int Foo()
                {
                    if (x)
                        return 0;

                    Bar();
                    return 1;
                }"""

        let fsharp = 
             """member this.Foo() =
                    if x then 0
                    else
                        Bar()
                        1"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert method attributes`` () = 
        let csharp = 
             """[CommandHandler (RefactoryCommands.QuickFix)]
                void OnQuickFixCommand ()
                {
                    if (!AnalysisOptions.EnableFancyFeatures || smartTagMarginMarker == null) {
                        //Fixes = RefactoringService.GetValidActions (Editor, DocumentContext, Editor.CaretLocation).Result;

                        PopupQuickFixMenu (null, null);
                        return;
                    }

                    CancelSmartTagPopupTimeout ();
                    PopupQuickFixMenu (null, menu => { });
                }"""

        let fsharp = 
             """[<CommandHandler(RefactoryCommands.QuickFix)>]
                member this.OnQuickFixCommand() =
                    if not AnalysisOptions.EnableFancyFeatures || smartTagMarginMarker = null then
                        PopupQuickFixMenu(null, null)
                        ()
                    else
                        CancelSmartTagPopupTimeout()
                        PopupQuickFixMenu(null, fun menu -> ())"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert multiple if statements with early returns`` () = 
        let csharp = 
             """public int Foo()
                {
                    if (x)
                        return 0;

                    Baz();
                    if (y)
                    {
                        return 42;
                    }
                    else 
                    {
                        Bar();
                    }
                                        
                    return 1;
                }"""

        let fsharp = 
             """member this.Foo() =
                    if x then 0
                    else
                        Baz()
                        if y then 42
                        else Bar()
                        1"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``can convert class constructor with subclass args`` () = 
        let csharp = 
             """[Activity(Label = "Activity A", MainLauncher = true)]
                public class MainApplication : MvxAndroidApplication
                {
                    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
                    {
                    }
                } """

        let fsharp = 
             """[<Activity(Label = "Activity A", MainLauncher = True)>]
                type MainApplication(javaReference: IntPtr, transfer: JniHandleOwnership) =
                    inherit MvxAndroidApplication(javaReference, transfer)"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert convert not statement`` () = 
        let csharp = 
            """
                public bool IsPlayServicesAvailable ()
                {
                    int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
                    if (resultCode != ConnectionResult.Success)
                    {
                        if (GoogleApiAvailability.Instance.IsUserResolvableError (resultCode))
                            msgText.Text = GoogleApiAvailability.Instance.GetErrorString (resultCode);
                        else
                        {
                            msgText.Text = "This device is not supported";
                            Finish ();
                        }
                        return false;
                    }
                    else
                    {
                        msgText.Text = "Google Play Services is available.";
                        return true;
                    }
                }"""

        let fsharp = 
             """member this.IsPlayServicesAvailable() =
                    let mutable resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this)
                    if resultCode <> ConnectionResult.Success then
                        if GoogleApiAvailability.Instance
                            .IsUserResolvableError(resultCode) then
                            msgText.Text <- GoogleApiAvailability.Instance.GetErrorString(resultCode)
                        else
                            msgText.Text <- "This device is not supported"
                            Finish()
                        False
                    else
                        msgText.Text <- "Google Play Services is available."
                        True"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert convert lambda`` () = 
        let csharp = 
             """public void Include(INotifyCollectionChanged changed)
                {
                    changed.CollectionChanged += (s, e) => { var test = $"Args: {e.Action}{e.NewItems}{e.OldItems}, Index: {e.OldStartingIndex}"; };
                }"""

        let fsharp = 
             """member this.Include(changed: INotifyCollectionChanged) =
                    changed.CollectionChanged.AddHandler<_> (fun (s, e) ->
                        let mutable test =
                            sprintf "Args: %O%O%O, Index: %O" (e.Action) (e.NewItems) (e.OldItems) (e.OldStartingIndex)
                        ())"""
                       
        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)



    [<Test>]
    member this.``convert full file - namespace with class`` () = 
        let csharp = 
         """using System;
            using System.Collections.Specialized;
            using System.Windows.Input;
            using Android.App;
            using Android.Views;
            using Android.Widget;
            using MvvmCross.Binding.BindingContext;
            using MvvmCross.Navigation;
            using MvvmCross.ViewModels;

            namespace StarWarsSample.Forms.Droid
            {
                // This class is never actually executed, but when Xamarin linking is enabled it does how to ensure types and properties
                // are preserved in the deployed app
                [Android.Runtime.Preserve(AllMembers = true)]
                public class LinkerPleaseInclude
                {
                    public void Include(Button button)
                    {
                        button.Click += (s, e) => button.Text = button.Text + "";
                    }

                public void Include(CheckBox checkBox)
                {
                    checkBox.CheckedChange += (sender, args) => checkBox.Checked = !checkBox.Checked;
                }

                public void Include(Switch @switch)
                {
                    @switch.CheckedChange += (sender, args) => @switch.Checked = !@switch.Checked;
                }

                public void Include(View view)
                {
                    view.Click += (s, e) => view.ContentDescription = view.ContentDescription + "";
                }

                public void Include(TextView text)
                {
                    text.AfterTextChanged += (sender, args) => text.Text = "" + text.Text;
                    text.Hint = "" + text.Hint;
                }

                public void Include(CheckedTextView text)
                {
                    text.AfterTextChanged += (sender, args) => text.Text = "" + text.Text;
                    text.Hint = "" + text.Hint;
                }

                public void Include(CompoundButton cb)
                {
                    cb.CheckedChange += (sender, args) => cb.Checked = !cb.Checked;
                }

                public void Include(SeekBar sb)
                {
                    sb.ProgressChanged += (sender, args) => sb.Progress = sb.Progress + 1;
                }

                public void Include(RadioGroup radioGroup)
                {
                    radioGroup.CheckedChange += (sender, args) => radioGroup.Check(args.CheckedId);
                }

                public void Include(RadioButton radioButton)
                {
                    radioButton.CheckedChange += (sender, args) => radioButton.Checked = args.IsChecked;
                }

                public void Include(RatingBar ratingBar)
                {
                    ratingBar.RatingBarChange += (sender, args) => ratingBar.Rating = 0 + ratingBar.Rating;
                }

                public void Include(Activity act)
                {
                    act.Title = act.Title + "";
                }

                public void Include(INotifyCollectionChanged changed)
                {
                    changed.CollectionChanged += (s, e) => { var test = $"{e.Action}{e.NewItems}{e.NewStartingIndex}{e.OldItems}"; };
                }

                public void Include(ICommand command)
                {
                    command.CanExecuteChanged += (s, e) => { if (command.CanExecute(null)) command.Execute(null); };
                }

                public void Include(MvvmCross.IoC.MvxPropertyInjector injector)
                {
                    injector = new MvvmCross.IoC.MvxPropertyInjector();
                }

                public void Include(System.ComponentModel.INotifyPropertyChanged changed)
                {
                    changed.PropertyChanged += (sender, e) =>
                    {
                        var test = e.PropertyName;
                    };
                }

                public void Include(MvxTaskBasedBindingContext context)
                {
                    context.Dispose();
                    var context2 = new MvxTaskBasedBindingContext();
                    context2.Dispose();
                }

                public void Include(MvxNavigationService service, IMvxViewModelLoader loader)
                {
                    service = new MvxNavigationService(null, loader);
                }

                public void Include(ConsoleColor color)
                {
                    Console.Write("");
                    Console.WriteLine("");
                    color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                public void Include(MvvmCross.Plugin.Json.Plugin plugin)
                {
                    plugin.Load();
                }
            }
        }"""

        let fsharp = 
             """namespace StarWarsSample.Forms.Droid

                open MvvmCross.ViewModels
                open MvvmCross.Navigation
                open MvvmCross.Binding.BindingContext
                open Android.Widget
                open Android.Views
                open Android.App
                open System.Windows.Input
                open System
                open System.Collections.Specialized

                [<Android.Runtime.Preserve(AllMembers = True)>]
                type LinkerPleaseInclude() =
                    member this.Include(button: Button) = button.Click.AddHandler<_> (fun (s, e) -> button.Text <- button.Text + "")
                    member this.Include(checkBox: CheckBox) =
                        checkBox.CheckedChange.AddHandler<_> (fun (sender, args) -> checkBox.Checked <- not checkBox.Checked)
                    member this.Include((@switch): Switch) =
                        (@switch.CheckedChange).AddHandler<_> (fun (sender, args) -> ``@switch``.Checked <- not (@switch).Checked)
                    member this.Include(view: View) =
                        view.Click.AddHandler<_> (fun (s, e) -> view.ContentDescription <- view.ContentDescription + "")

                    member this.Include(text: TextView) =
                        text.AfterTextChanged.AddHandler<_> (fun (sender, args) -> text.Text <- "" + text.Text)
                        text.Hint <- "" + text.Hint

                    member this.Include(text: CheckedTextView) =
                        text.AfterTextChanged.AddHandler<_> (fun (sender, args) -> text.Text <- "" + text.Text)
                        text.Hint <- "" + text.Hint

                    member this.Include(cb: CompoundButton) =
                        cb.CheckedChange.AddHandler<_> (fun (sender, args) -> cb.Checked <- not cb.Checked)
                    member this.Include(sb: SeekBar) =
                        sb.ProgressChanged.AddHandler<_> (fun (sender, args) -> sb.Progress <- sb.Progress + 1)
                    member this.Include(radioGroup: RadioGroup) =
                        radioGroup.CheckedChange.AddHandler<_> (fun (sender, args) -> radioGroup.Check(args.CheckedId))
                    member this.Include(radioButton: RadioButton) =
                        radioButton.CheckedChange.AddHandler<_> (fun (sender, args) -> radioButton.Checked <- args.IsChecked)
                    member this.Include(ratingBar: RatingBar) =
                        ratingBar.RatingBarChange.AddHandler<_> (fun (sender, args) -> ratingBar.Rating <- 0 + ratingBar.Rating)
                    member this.Include(act: Activity) = act.Title <- act.Title + ""

                    member this.Include(changed: INotifyCollectionChanged) =
                        changed.CollectionChanged.AddHandler<_> (fun (s, e) ->
                            let mutable test = sprintf "%O%O%O%O" (e.Action) (e.NewItems) (e.NewStartingIndex) (e.OldItems)
                            ())

                    member this.Include(command: ICommand) =
                        command.CanExecuteChanged.AddHandler<_> (fun (s, e) ->
                            if command.CanExecute(null) then command.Execute(null))

                    member this.Include(injector: MvvmCross.IoC.MvxPropertyInjector) =
                        injector <- new MvvmCross.IoC.MvxPropertyInjector()

                    member this.Include(changed: System.ComponentModel.INotifyPropertyChanged) =
                        changed.PropertyChanged.AddHandler<_> (fun (sender, e) ->
                            let mutable test = e.PropertyName
                            ())

                    member this.Include(context: MvxTaskBasedBindingContext) =
                        context.Dispose()
                        let mutable context2 = new MvxTaskBasedBindingContext()
                        context2.Dispose()

                    member this.Include(service: MvxNavigationService, loader: IMvxViewModelLoader) =
                        service <- new MvxNavigationService(null, loader)

                    member this.Include(color: ConsoleColor) =
                        Console.Write("")
                        Console.WriteLine("")
                        color <- Console.ForegroundColor
                        Console.ForegroundColor <- ConsoleColor.Red
                        Console.ForegroundColor <- ConsoleColor.Yellow
                        Console.ForegroundColor <- ConsoleColor.Magenta
                        Console.ForegroundColor <- ConsoleColor.White
                        Console.ForegroundColor <- ConsoleColor.Gray
                        Console.ForegroundColor <- ConsoleColor.DarkGray

                    member this.Include(plugin: MvvmCross.Plugin.Json.Plugin) = plugin.Load()""" 

        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert class with generic types`` () = 
        let csharp = 
             """public partial class TipView<T, Z> : MvxContentPage<TipViewModel>
                {
                    public TipView(string s, int i) : base(message)
                    {
                        InitializeComponent();
                    }
                }"""

        let fsharp = 
             """type TipView<'T, 'Z>(s: string, i: int) =
                    inherit MvxContentPage<TipViewModel>(message)"""

        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert class with interface beginning with I as interface`` () = 
        let csharp = 
             """public class Foo : IDisposable
                {
                    public void Dispose()
                    {
                        FooBar();
                    }
                }"""

        let fsharp = 
             """type Foo() =
                    member this.Dispose() = FooBar()
                    interface IDisposable with
                        member this.Todo() = ()"""

        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    [<Test>]
    member this.``convert class with multiple interfaces`` () = 
        let csharp = 
             """public class Foo : IDisposable, IFoo
                {
                    public void Dispose()
                    {
                        FooBar();
                    }
                }"""

        let fsharp = 
             """type Foo() =
                    member this.Dispose() = FooBar()

                    interface IDisposable with
                        member this.Todo() = ()

                    interface IFoo with
                        member this.Todo() = ()"""

        csharp |> Converter.run 
        |> (fun x -> printfn "%s" x; x)
        |> should equal (formatFsharp fsharp)

    // TODO: this test sis currently failing. 
    //[<Test>]
    //member this.``convert continue and foreach loop`` () = 
        //let csharp = 
        //     """ContextMenu CreateContextMenu (CodeFixMenu entrySet)
        //        {
        //            var menu = new ContextMenu ();
        //            foreach (var item in entrySet.Items) {
        //                if (item == CodeFixMenuEntry.Separator) {
        //                    menu.Items.Add (new SeparatorContextMenuItem ());
        //                    continue;
        //                }

        //                var menuItem = new ContextMenuItem (item.Label);
        //                menuItem.Context = item.Action;
        //                if (item.Action == null) {
        //                    if (!(item is CodeFixMenu itemAsMenu) || itemAsMenu.Items.Count <= 0) {
        //                        menuItem.Sensitive = false;
        //                    }
        //                }
        //                var subMenu = item as CodeFixMenu;
        //                if (subMenu != null) {
        //                    menuItem.SubMenu = CreateContextMenu (subMenu);
        //                    menuItem.Selected += delegate {
        //                        RefactoringPreviewTooltipWindow.HidePreviewTooltip ();
        //                    };
        //                    menuItem.Deselected += delegate { RefactoringPreviewTooltipWindow.HidePreviewTooltip (); };
        //                } else {
        //                    menuItem.Clicked += (sender, e) => ((System.Action)((ContextMenuItem)sender).Context) ();
        //                    menuItem.Selected += (sender, e) => {
        //                        RefactoringPreviewTooltipWindow.HidePreviewTooltip ();
        //                        if (item.ShowPreviewTooltip != null) {
        //                            item.ShowPreviewTooltip (e);
        //                        }
        //                    };
        //                    menuItem.Deselected += delegate { RefactoringPreviewTooltipWindow.HidePreviewTooltip (); };
        //                }
        //                menu.Items.Add (menuItem);
        //            }
        //            menu.Closed += delegate { RefactoringPreviewTooltipWindow.HidePreviewTooltip (); };
        //            return menu;
        //        }"""
    
        //let fsharp = 
        //     """member this.SendNotification(messageBody: string) =
        //            let mutable notificationBuilder =
        //                new Notification.Builder(this).SetContentTitle("FCM Message").SetSmallIcon(Resource.Drawable.ic_launcher)
        //                    .SetContentText(messageBody).SetAutoCancel(True).SetContentIntent(pendingIntent)
        //            let mutable notificationManager = NotificationManager.FromContext(this)
        //            notificationManager.Notify(0, notificationBuilder.Build())"""
                   
        //csharp |> Converter.run 
        //|> (fun x -> printfn "%s" x; x)
        //|> should equal (formatFsharp fsharp)