import { newArray } from '@angular/compiler/src/util';
import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { forkJoin, Observable } from 'rxjs';
import { RecommendationFilterOptions, RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';
import { UserWatchlistService } from './user-watchlist.service';

@Component({
  selector: 'gm-user-watchlist',
  templateUrl: './user-watchlist.component.html',
  styleUrls: ['./user-watchlist.component.scss']
})
export class UserWatchlistComponent implements OnInit {

  public selectedItem?: StockRecommendation;
  public isRefreshing: boolean;
  public isSearching: boolean;
  public searchControl = new FormControl('', []);
  public missingSymbols: string[] = [];

  public exportFileUrl!: SafeResourceUrl;
  public signalSelection = new FormControl();
  public signalFilterList: string[] = ['Strong Buy', 'Buy', 'Hold', 'Sell', 'Strong Sell'];
  
  constructor(public userWatchlistService: UserWatchlistService,
    private stockRecommendationService: StockRecommendationService,
    private sanitizer: DomSanitizer) {
    this.isRefreshing = false;
    this.isSearching = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  public refresh() {
    this.isRefreshing = true;
    this.userWatchlistService.refreshWatchlist().subscribe(x => {
      this.isRefreshing = false;
      this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.userWatchlistService.watchlist.exportSorted()));
    });
  }

  public filterRecommendations() {
   const searchString = (this.searchControl.value as string).split(' ').filter(x => x !== '');
    this.missingSymbols = [];
    for(let symbol of searchString) {
      if(!this.userWatchlistService.watchlist.includes(symbol)) {
        this.missingSymbols.push(symbol);
      }
    }
    
    let signalFilters = this.signalSelection.value as Array<string>;
    if(signalFilters == null) {
      signalFilters = new Array<string>();
    }
    const filter = new RecommendationFilterOptions(searchString, signalFilters);


    this.userWatchlistService.watchlist.applyFilter(filter);
    this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.userWatchlistService.watchlist.exportSorted()));
    if(this.userWatchlistService.watchlist.sorted.length > 0) {
      this.selectedItem = this.userWatchlistService.watchlist.sorted[0];
    }
  }

  public onSearchKeyup(event: KeyboardEvent) {
    this.searchControl.setErrors(null);
  }

  public search() {
    if(this.missingSymbols.length == 0) {
      return;
    }
    
    this.signalSelection.setValue(new Array<string>());
    this.isSearching = true;
    this.selectedItem = undefined;
    let recommendationsSearch = new Array<Observable<StockRecommendation>>();
    for(let symbol of this.missingSymbols) {
      recommendationsSearch.push(this.stockRecommendationService.getRecommendationFor(symbol));
    }

    forkJoin(recommendationsSearch).subscribe(recommendations => {
      for(const r of recommendations) {
        this.userWatchlistService.watchlist.add(r);
      }
      this.selectedItem = recommendations[0];
      this.isSearching = false;
      this.missingSymbols = [];
    }, error => {
      this.searchControl.setErrors({'notFound': true});
      this.isSearching = false;
    });
  }

  public getSearchResultErrorMessage() {
    if (this.searchControl.hasError('notFound')) {
      return "Couldn't find symbol(s): " + this.missingSymbols.join(', ').toUpperCase();
    } else if(this.searchControl.hasError('duplicate')) {
      return 'Stock is already in the daily list';
    }
    return '';
  }

  public getSearchMessage() {
    return 'Searching for ' + this.missingSymbols.join(', ').toUpperCase() + '...';
  }
}
