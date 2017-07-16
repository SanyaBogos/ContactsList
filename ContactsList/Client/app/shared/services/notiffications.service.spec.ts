import { TestBed, inject } from '@angular/core/testing';

import { NotifficationsService } from './notiffications.service';

describe('NotifficationsService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NotifficationsService]
    });
  });

  it('should ...', inject([NotifficationsService], (service: NotifficationsService) => {
    expect(service).toBeTruthy();
  }));
});
