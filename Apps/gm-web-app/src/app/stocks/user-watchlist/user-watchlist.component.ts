import { newArray } from '@angular/compiler/src/util';
import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
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
  
  public exportFileUrl!: SafeResourceUrl;
  signalSelection = new FormControl();
  signalFilterList: string[] = ['Strong Buy', 'Buy', 'Hold', 'Sell', 'Strong Sell'];
  
  constructor(public userWatchlistService: UserWatchlistService,
    private stockRecommendationService: StockRecommendationService,
    private sanitizer: DomSanitizer) {
    this.isRefreshing = false;
    this.isSearching = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  refresh() {
    this.isRefreshing = true;
    this.userWatchlistService.refreshWatchlist().subscribe(x => {
      this.isRefreshing = false;
      this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.userWatchlistService.watchlist.exportSorted()));
    });
  }

  filterRecommendations() {
    const searchString = this.searchControl.value as string;
    const signalFilters = this.signalSelection.value as Array<string>;
    const filter = new RecommendationFilterOptions(searchString.split(' ').filter(x => x !== ''), signalFilters);

    this.userWatchlistService.watchlist.applyFilter(filter);
    this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.userWatchlistService.watchlist.exportSorted()));
    if(this.userWatchlistService.watchlist.sorted.length > 0) {
      this.selectedItem = this.userWatchlistService.watchlist.sorted[0];
    }
  }

  onSearchKeyup(event: KeyboardEvent) {
    this.searchControl.setErrors(null);
  }

  search() {
    if(this.searchControl.value == '') {
      return;
    }
    if(this.userWatchlistService.includes(this.searchControl.value)) {
      this.selectedItem = this.userWatchlistService.watchlist.recommendations.find(x => x.symbol.toLowerCase() === this.searchControl.value.toLowerCase());
      return;
    }

    this.isSearching = true;
    this.stockRecommendationService.getRecommendationFor(this.searchControl.value).subscribe(x => {
      this.selectedItem = x;
      this.isSearching = false;
    }, error => {
      this.searchControl.setErrors({'notFound': true});
      this.isSearching = false;
    });
  }

  getSearchResultErrorMessage() {
    if (this.searchControl.hasError('notFound')) {
      return 'Stock not found';
    } else if(this.searchControl.hasError('duplicate')) {
      return 'Stock is already in the watchlist';
    }
    return '';
  }
}
