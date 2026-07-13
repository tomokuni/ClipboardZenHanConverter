namespace ClipboardZenHanConverter.Core.Interfaces;

public interface INavigationAware
{
    void OnNavigatedTo(object? parameter);
    void OnNavigatingFrom();
}
