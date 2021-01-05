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
  public signalFilterList: string[] = [];
  
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
    this.selectedItem = undefined;
    this.userWatchlistService.refreshWatchlist().subscribe(x => {
      this.isRefreshing = false;
      this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.userWatchlistService.watchlist.exportSorted()));
    });
  }

  public toggleFilter(event: MouseEvent, filter: string) {
    event.stopImmediatePropagation();
    const index = this.signalFilterList.indexOf(filter);
    if(index > -1) {
      this.signalFilterList.splice(index, 1);
    } else {
      this.signalFilterList.push(filter);
    }
    this.filterRecommendations();
  }

  public filterRecommendations() {
   const searchString = (this.searchControl.value as string).split(' ').filter(x => x !== '');
    this.missingSymbols = [];
    for(let symbol of searchString) {
      if(!this.userWatchlistService.watchlist.includes(symbol)) {
        this.missingSymbols.push(symbol);
      }
    }
    
    let signalFilters = this.signalFilterList;
    if(signalFilters == null) {
      signalFilters = new Array<string>();
    }
    const filter = new RecommendationFilterOptions(searchString, signalFilters);

    this.selectedItem = undefined;
    this.userWatchlistService.watchlist.applyFilter(filter);
    this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.userWatchlistService.watchlist.exportSorted()));
  }

  public onSearchKeyup(event: KeyboardEvent) {
    this.searchControl.setErrors(null);
  }

  public search() {
    if(this.missingSymbols.length == 0) {
      return;
    }
    
    this.signalFilterList = [];
    this.isSearching = true;
    this.selectedItem = undefined;
    let recommendationsSearch = new Array<Observable<StockRecommendation>>();
    for(let symbol of this.missingSymbols) {
      recommendationsSearch.push(this.stockRecommendationService.getRecommendationFor(symbol));
    }

    forkJoin(recommendationsSearch).subscribe(recommendations => {
      for(const r of recommendations) {
        this.userWatchlistService.addToWatchlist(r);
      }
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
