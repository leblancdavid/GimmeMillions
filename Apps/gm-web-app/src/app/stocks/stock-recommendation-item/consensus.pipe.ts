import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'consensus'
})
export class ConsensusPipe implements PipeTransform {

  transform(value: number): string {
    if(value >= 90) {
      return "STRONG BUY";
    } else if(value >= 75) {
      return "BUY";
    } else if(value > 25) {
      return "HOLD";
    } else if(value > 10) {
      return "SELL"; 
    } else {
      return "STRONG SELL";
    }
  }
}
