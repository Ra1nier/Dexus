// Hook into the beforeunload event
window.registerBeforeUnload = (dotNetHelper) =>
{
    window.addEventListener("beforeunload", (event) =>
    {
        dotNetHelper.invokeMethodAsync("OnApplicationExit");
    });
};