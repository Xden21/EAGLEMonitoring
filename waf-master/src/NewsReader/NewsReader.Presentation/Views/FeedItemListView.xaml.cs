﻿using Jbe.NewsReader.Applications.Services;
using Jbe.NewsReader.Applications.ViewModels;
using Jbe.NewsReader.Applications.Views;
using Jbe.NewsReader.Presentation.Controls;
using System;
using System.ComponentModel;
using System.Composition;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Jbe.NewsReader.Presentation.Views
{
    [Export(typeof(IFeedItemListView)), Shared]
    public sealed partial class FeedItemListView : IFeedItemListView
    {
        private readonly Lazy<FeedItemListViewModel> viewModel;
        private readonly ISelectionStateManager selectionStateManager;


        public FeedItemListView()
        {
            InitializeComponent();
            viewModel = new Lazy<FeedItemListViewModel>(() => (FeedItemListViewModel)DataContext);
            SetDefaultSearchVisibility();
            selectionStateManager = SelectionStateHelper.CreateManager(feedItemListView, selectItemsButton, cancelSelectionButton);
            selectionStateManager.PropertyChanged += SelectionStateManagerPropertyChanged;
            UpdateSelectionStateDependentVisibility();
            Loaded += FirstTimeLoaded;
        }
        

        public FeedItemListViewModel ViewModel => viewModel.Value;


        public void CancelMultipleSelectionMode() => selectionStateManager.CancelMultipleSelectionMode();

        private void FirstTimeLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FirstTimeLoaded;
            ViewModel.SelectionService.PropertyChanged += SelectionServicePropertyChanged;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var isControlKeyDown = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            if (!isControlKeyDown && e.Key == VirtualKey.F5)
            {
                ViewModel.RefreshCommand.Execute(null);
            }
            if (isControlKeyDown && e.Key == VirtualKey.U)
            {
                ViewModel.ReadUnreadCommand.Execute("unread");
            }
            else if (isControlKeyDown && e.Key == VirtualKey.Q)
            {
                ViewModel.ReadUnreadCommand.Execute("read");
            }
            else if (isControlKeyDown && e.Key == VirtualKey.F)
            {
                ShowSearch();
            }
        }

        private void FeedItemListViewItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ShowFeedItemViewCommand.Execute(e.ClickedItem);
        }

        private void FeedDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ViewModel.ShowFeedItemViewCommand.Execute(((FrameworkElement)sender).DataContext);
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e) => ShowSearch();

        private async void ShowSearch()
        {
            searchButton.Visibility = Visibility.Collapsed;
            searchBox.Visibility = Visibility.Visible;
            await Dispatcher.RunIdleAsync(ha => { });
            await Task.Delay(25);
            searchBox.Focus(FocusState.Programmatic);
        }

        private void SetDefaultSearchVisibility()
        {
            searchButton.Visibility = Visibility.Visible;
            searchBox.Visibility = Visibility.Collapsed;
        }

        private void SearchBoxLostFocus(object sender, RoutedEventArgs e) => SetDefaultSearchVisibility();

        private async void SelectionServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SelectionService.SelectedFeedItem)) return;

            await Dispatcher.RunIdleAsync(ha => { });  // Ensure that items are layouted first so that ScrollIntoView works correct.

            if (ViewModel.SelectionService.SelectedFeedItem != null)
            {
                feedItemListView.ScrollIntoView(ViewModel.SelectionService.SelectedFeedItem);

                // When the element is not yet available (virtualized) then scroll into view again
                if (feedItemListView.ContainerFromItem(ViewModel.SelectionService.SelectedFeedItem) == null)
                {
                    await Dispatcher.RunIdleAsync(ha => { });
                    feedItemListView.ScrollIntoView(ViewModel.SelectionService.SelectedFeedItem);
                }
            }
        }

        private void SelectionStateManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISelectionStateManager.SelectionState))
            {
                UpdateSelectionStateDependentVisibility();
            }
        }

        private void UpdateSelectionStateDependentVisibility()
        {
            markAsReadUnreadButton.Visibility = selectionStateManager.SelectionState == SelectionState.Master ? Visibility.Collapsed : Visibility.Visible;
            allFeedItems.Visibility = selectionStateManager.SelectionState == SelectionState.MultipleSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private static FontWeight GetFontWeight(bool markAsRead) => markAsRead ? FontWeights.Normal : FontWeights.SemiBold;
    }
}
