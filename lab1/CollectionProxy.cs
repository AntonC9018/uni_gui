using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace CarApp;

public class CollectionProxy<Model, Observable> : ICollection<Model>
{
    private List<Model> _underlyingCollection;
    private ObservableCollection<Observable> _underlyingObservable;
    private Func<Model, Observable> _factory;

    public CollectionProxy(List<Model> cars, ObservableCollection<Observable> bindings, Func<Model, Observable> factory)
    {
        _underlyingCollection = cars;
        _underlyingObservable = bindings;
        _factory = factory;

        bindings.CollectionChanged += (sender, e) =>
        {
            Debug.Assert(sender is not null);
            var collection = (ObservableCollection<Observable>) sender;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                {
                    var t = _underlyingCollection[e.OldStartingIndex];
                    _underlyingCollection[e.OldStartingIndex] = _underlyingCollection[e.NewStartingIndex];
                    _underlyingCollection[e.NewStartingIndex] = t;
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    _underlyingCollection.RemoveAt(e.OldStartingIndex);
                    break;
                }
                default:
                {
                    Debug.Fail("Unsupported case " + e.Action);
                    break;
                }
            }
        };
    }

    public int Count => _underlyingCollection.Count;
    public bool IsReadOnly => false;

    public void Add(Model item)
    {
        _underlyingCollection.Add(item);
        _underlyingObservable.Add(_factory(item));
    }

    public void Clear() => _underlyingCollection.Clear();
    public bool Contains(Model item) => _underlyingCollection.Contains(item);
    public void CopyTo(Model[] array, int arrayIndex) => _underlyingCollection.CopyTo(array, arrayIndex);
    public IEnumerator<Model> GetEnumerator() => _underlyingCollection.GetEnumerator();
    public bool Remove(Model item)
    {
        var index = _underlyingCollection.IndexOf(item);
        if (index == -1)
            return false;
        _underlyingObservable.RemoveAt(index);
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


// https://stackoverflow.com/a/2177659/9731532
/* 
    Click button to create
    -> add a new model to models
    -> trigger add event on the model observable
    -> add a new view model to view models
    -> trigger add view model event
    -> update UI

    UI value changed
    -> set property on view model via binding
    -> set field on model
    -> trigger update event
    -> update UI (validation for example)
*/
public class ObservableViewModelCollection<TViewModel, TModel> : ObservableCollection<TViewModel>
{
    private readonly ObservableCollection<TModel> _source;
    private readonly Func<TModel, TViewModel> _viewModelFactory;

    public ObservableViewModelCollection(ObservableCollection<TModel> source, Func<TModel, TViewModel> viewModelFactory)
        : base(source.Select(model => viewModelFactory(model)))
    {
        Contract.Requires(source != null);
        Contract.Requires(viewModelFactory != null);

        _source = source;
        _viewModelFactory = viewModelFactory;
        _source.CollectionChanged += OnSourceCollectionChanged;
    }

    // The interface of NotifyCollectionChangedEventArgs isn't obvious at all,
    // this implementation is one that made sense.
    private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                    this.Insert(e.NewStartingIndex + i, _viewModelFactory((TModel) e.NewItems[i]));
                break;
            }

            case NotifyCollectionChangedAction.Move:
            {
                if (e.OldItems.Count == 1)
                {
                    this.Move(e.OldStartingIndex, e.NewStartingIndex);
                }
                else
                {
                    // This part is kinda terribly implemented though.
                    // I might reimplement this part.
                    List<TViewModel> items = this.Skip(e.OldStartingIndex).Take(e.OldItems.Count).ToList();
                    for (int i = 0; i < e.OldItems.Count; i++)
                        this.RemoveAt(e.OldStartingIndex);

                    for (int i = 0; i < items.Count; i++)
                        this.Insert(e.NewStartingIndex + i, items[i]);
                }
                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                    this.RemoveAt(e.OldStartingIndex);
                break;
            }

            case NotifyCollectionChangedAction.Replace:
            {
                // These things also could be done better, if it's possible to reuse the view models.
                
                // remove
                for (int i = 0; i < e.OldItems.Count; i++)
                    this.RemoveAt(e.OldStartingIndex);

                // add
                goto case NotifyCollectionChangedAction.Add;
            }

            case NotifyCollectionChangedAction.Reset:
            {
                Clear();
                for (int i = 0; i < e.NewItems.Count; i++)
                    this.Add(_viewModelFactory((TModel) e.NewItems[i]));
                break;
            }

            default:
                break;
        }
    }
}