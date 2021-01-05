import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { forkJoin, Observable } from 'rxjs';
import { RecommendationFilterOptions, RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-daily-predictions',
  templateUrl: './daily-predictions.component.html',
  styleUrls: ['./daily-predictions.component.scss']
})
export class DailyPredictionsComponent implements OnInit {

  public predictions: RecommendationList;
  public selectedItem?: StockRecommendation;
  public isRefreshing: boolean; 
  public isSearching: boolean;
  public searchControl = new FormControl('', []);
  public missingSymbols: string[] = [];
  
  public exportFileUrl!: SafeResourceUrl;

  public signalFilterList: string[] = [];

  constructor(private stockRecommendationService: StockRecommendationService,
    private sanitizer: DomSanitizer) {
    this.predictions = new RecommendationList();
    this.isRefreshing = false;
    this.isSearching = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  public refresh() {
    this.isRefreshing = true;
    this.predictions = new RecommendationList();
    this.stockRecommendationService.getDailyPicks().subscribe(x => {
      this.isRefreshing = false;
      this.predictions.recommendations = x;
      this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.predictions.exportSorted()));
      if(this.predictions.recommendations.length > 0) {
        this.selectedItem = this.predictions.recommendations[0];
      }
    }, error => {
      this.isRefreshing = false;
    });
  }

  public toggleFilter(event: Event, filter: string) {
    const index = this.signalFilterList.indexOf(filter);
    if(index > -1) {
      this.signalFilterList = this.signalFilterList.splice(index, 1);
    } else {
      this.signalFilterList.push(filter);
    }
    this.filterRecommendations();
  }

  public filterRecommendations() {
    const searchString = (this.searchControl.value as string).split(' ').filter(x => x !== '');
    this.missingSymbols = [];
    for(let symbol of searchString) {
      if(!this.predictions.includes(symbol)) {
        this.missingSymbols.push(symbol);
      }
    }
    
    let signalFilters = this.signalFilterList;
    if(signalFilters == null) {
      signalFilters = new Array<string>();
    }
    const filter = new RecommendationFilterOptions(searchString, signalFilters);

    this.predictions.applyFilter(filter);
    this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.predictions.exportSorted()));
    if(this.predictions.sorted.length > 0) {
      this.selectedItem = this.predictions.sorted[0];
    }
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
        this.predictions.add(r);
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
