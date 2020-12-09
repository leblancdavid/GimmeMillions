import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { RecommendationList } from '../recommendation-list/recommendation-list';
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
  
  public exportFileUrl!: SafeResourceUrl;

  constructor(private stockRecommendationService: StockRecommendationService,
    private sanitizer: DomSanitizer) {
    this.predictions = new RecommendationList();
    this.isRefreshing = false;
    this.isSearching = false;
   }

  ngOnInit(): void {
    this.refresh();
  }

  refresh() {
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

  onFilterKeyup(event: Event) {
    this.predictions.applyFilter((event.target as HTMLInputElement).value);
    this.exportFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(window.URL.createObjectURL(this.predictions.exportSorted()));
    if(this.predictions.sorted.length > 0) {
      this.selectedItem = this.predictions.sorted[0];
    }
  }

  onSearchKeyup(event: KeyboardEvent) {
    this.searchControl.setErrors(null);
  }

  search() {
    if(this.searchControl.value == '') {
      return;
    }

    if(this.predictions.includes(this.searchControl.value)) {
      this.selectedItem = this.predictions.recommendations.find(x => x.symbol.toLowerCase() === this.searchControl.value.toLowerCase());
      return;
    }

    this.isSearching = true;
    this.stockRecommendationService.getRecommendationFor(this.searchControl.value).subscribe(x => {
      this.predictions.add(x);
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
      return 'Stock is already in the daily list';
    }
    return '';
  }
}
